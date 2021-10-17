using System;
using System.Collections.Generic;
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

    public static KeyboardHook Instance;                                        // Access from anywhere similar to input module
    static protected IntPtr hHook = IntPtr.Zero;                                // null pointers woo
    protected delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);    // delegate for our low level keyboard function

    // State 0 = not pressed
    // State 1 = recently pressed
    // State 2 = held down
    // State 3 = recently released
    // State 4 = recently released but transitioning to 0
    private static Dictionary<uint,uint> keyStates;                             // contains states of any keys that have been pressed

    // create the hook and init the key states
    void Start()
    {
        // init
        Instance = this;
        keyStates = new Dictionary<uint, uint>();
        bool result = InitHook();
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

    // reset
    private void Update()
    {
        foreach (uint key in new List<uint>(keyStates.Keys))
        {
            if (keyStates[key] == 4)
            {
                keyStates[key] = 0;
            }
        }
    }

    // flag to reset
    private void LateUpdate()
    {
        // update state or flag to reset
        // making sure that up and down are only called once
        foreach(uint key in new List<uint>(keyStates.Keys))
        {
            if(keyStates[key] == 3)
            {
                keyStates[key] = 4;
            }
            else if(keyStates[key] == 1)
            {
                keyStates[key] = 2;
            }
        }
    }

    /**
     * Returns whether or not the given virtual key has been pressed
     */
    public bool GetKey(uint vk)
    {
        if(keyStates.ContainsKey(vk))
        {
            return keyStates[vk] == 1;
        }
        return false;
    }

    /**
     * Returns whether or not the given virtual key is held down
     */
    public bool GetKeyDown(uint vk)
    {
        if (keyStates.ContainsKey(vk))
        {
            return keyStates[vk] == 1 || keyStates[vk] == 2;
        }
        return false;
    }

    /**
     * Returns whether or not the given virtual key has been released
     */
    public bool GetKeyUp(uint vk)
    {
        if (keyStates.ContainsKey(vk))
        {
            return keyStates[vk] == 3 || keyStates[vk] == 4;
        }
        return false;
    }

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

        // update the keycode according to the keyboard event that happened

        // key is down
        if (wParam.ToInt64() == 0x0100)
        {
            if (keyStates.ContainsKey(hookStruct.vkCode))
            {
                if (keyStates[hookStruct.vkCode] == 1 || keyStates[hookStruct.vkCode] == 2)
                {
                    keyStates[hookStruct.vkCode] = 2;
                }
                else
                {
                    keyStates[hookStruct.vkCode] = 1;
                }
            }
            else
            {
                keyStates.Add(hookStruct.vkCode, 1);
            }
        }
        else if (wParam.ToInt64() == 0x0101)
        {
            if(keyStates.ContainsKey(hookStruct.vkCode))
            {
                keyStates[hookStruct.vkCode] = 3;
            }
            else
            {
                keyStates.Add(hookStruct.vkCode, 3);
            } 
        }
        
        // return and pass to the next hook
        return CallNextHookEx(hHook, 0, wParam, lParam);
    }
}
