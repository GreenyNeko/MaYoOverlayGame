using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class KeyboardHook : MonoBehaviour
{
    // Import the function to create hooks
    [DllImport("user32.dll")]
    protected static extern IntPtr SetWindowsHookEx(HookType code, HookProc func, IntPtr hInstance, int threadID);

    // import the function to remove hooks
    [DllImport("user32.dll")]
    protected static extern int UnhookWindowsHookEx(IntPtr hHook);

    // import the function to pass from one to the other hook
    [DllImport("user32.dll")]
    protected static extern int CallNextHookEx(IntPtr hHook, int code, IntPtr wParam, IntPtr lParam);


    // the types of hooks we have at our disposal
    protected enum HookType : int
    {
        WH_JOURNALRECORD = 0,
        WH_JOURNALPLAYBACK = 1,
        WH_KEYBOARD = 2,
        WH_GETMESSAGE = 3,
        WH_CALLWNDPROC = 4,
        WH_CBT = 5,
        WH_SYSMSGFILTER = 6,
        WH_MOUSE = 7,
        WH_HARDWARE = 8,
        WH_DEBUG = 9,
        WH_SHELL = 10,
        WH_FOREGROUNDIDLE = 11,
        WH_CALLWNDPROCRET = 12,
        WH_KEYBOARD_LL = 13,
        WH_MOUSE_LL = 14
    }

    // the struct for global keyboard hooks defined by windows
    struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    static protected IntPtr hHook = IntPtr.Zero;                                // null pointers woo
    private static bool[] keyStates = new bool[4];                              // keep track of the state of our keys
    protected delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);    // delegate for our low level keyboard function

    // create the hook
    protected bool InitHook()
    {
        if (hHook == IntPtr.Zero)
        {
            hHook = SetWindowsHookEx(HookType.WH_KEYBOARD_LL, LowLevelKeyboardProc, IntPtr.Zero, 0); //(int)AppDomain.GetCurrentThreadId() apparently 0 for LL?
        }
        return hHook != IntPtr.Zero;
    }

    // remove the hook
    protected void DestroyHook()
    {
        if (hHook != IntPtr.Zero)
        {
            UnhookWindowsHookEx(hHook);
            hHook = IntPtr.Zero;
        }
    }

    // create the hook and init the key states
    void Start()
    {
        bool result = InitHook();
        for(int i = 0; i < keyStates.Length; i++)
        {
            keyStates[i] = false;
        }
    }

    // OnDisable should be called before OnDestroy?
    private void OnDestroy()
    {
        //DestroyHook();
    }

    // remove the hook when this object is disabled
    private void OnDisable()
    {
        DestroyHook();
    }

    // add the hook when the object is enabled, superseded by Start(?)
    private void OnEnable()
    {
        //InitHook(LowLevelKeyboardProc);
    }

    // Our low level keyboard procedure
    [AOT.MonoPInvokeCallback(typeof(HookProc))]
    private static int LowLevelKeyboardProc(int code, IntPtr wParam, IntPtr lParam)
    {
        // ignore if code is negative, I forgot exactly why, but events with negative codes is not what we want
        if (code < 0)
        {
            // pass it to the next hook
            return CallNextHookEx(hHook, code, wParam, lParam);
        }
        // copy data over to our struct
        KBDLLHOOKSTRUCT hookStruct = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

        // do tings respective to the key that has been pressed
        // we check if the key has been recently pressed or has already been pressed and whether or not it has been released or pressed
        // 0x0100 is down event and 0x0101 is up event
        // 0x61 is numpad_1, 0x62 is numpad_2, 0x63 is numpad_3, 0x1b is escape
        // we call the respective event in game manager then

        // TODO: don't call functions inside of here that do stuff but instead grab the content and return to prevent being shutdowned by windows for not returning fast enough
        // that approach prevents crashes and makes debugging easier
        switch (hookStruct.vkCode)
        {
            case 0x61:
                if(wParam.ToInt64() == 0x0100)
                {
                    if(!keyStates[0])
                    {
                        GameManager.Instance.OnCorrect();
                        keyStates[0] = true;
                    }
                }
                else if(wParam.ToInt64() == 0x0101)
                {
                    keyStates[0] = false;
                }
                break;
            case 0x62:
                if (wParam.ToInt64() == 0x0100)
                {
                    if (!keyStates[1])
                    {
                        GameManager.Instance.OnSloppy();
                        keyStates[1] = true;
                    }
                }
                else if (wParam.ToInt64() == 0x0101)
                {
                    keyStates[1] = false;
                }
                break;
            case 0x63:
                if (wParam.ToInt64() == 0x0100)
                {
                    if (!keyStates[2])
                    {
                        GameManager.Instance.OnMiss();
                        keyStates[2] = true;
                    }
                }
                else if (wParam.ToInt64() == 0x0101)
                {
                    keyStates[2] = false;
                }
                break;
            case 0x1B:
                if (wParam.ToInt64() == 0x0100)
                {
                    if (!keyStates[3])
                    {
                        GameManager.Instance.OnEnd();
                        keyStates[3] = true;
                    }
                }
                else if (wParam.ToInt64() == 0x0101)
                {
                    keyStates[3] = false;
                }
                break;
        }
        
        // return and pass to the next hook
        return CallNextHookEx(hHook, 0, wParam, lParam);
    }
}
