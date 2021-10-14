using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class KeyboardHook : MonoBehaviour
{
    [DllImport("user32.dll")]
    protected static extern IntPtr SetWindowsHookEx(HookType code, HookProc func, IntPtr hInstance, int threadID);

    [DllImport("user32.dll")]
    protected static extern int UnhookWindowsHookEx(IntPtr hHook);
    [DllImport("user32.dll")]
    protected static extern int CallNextHookEx(IntPtr hHook, int code, IntPtr wParam, IntPtr lParam);

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

    struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    static protected IntPtr hHook = IntPtr.Zero;
    private static bool[] keyStates = new bool[4];
    protected delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);

    protected bool InitHook()
    {
        if (hHook == IntPtr.Zero)
        {
            hHook = SetWindowsHookEx(HookType.WH_KEYBOARD_LL, LowLevelKeyboardProc, IntPtr.Zero, 0); //(int)AppDomain.GetCurrentThreadId() apparently 0 for LL?
        }
        return hHook != IntPtr.Zero;
    }

    protected void DestroyHook()
    {
        if (hHook != IntPtr.Zero)
        {
            UnhookWindowsHookEx(hHook);
            hHook = IntPtr.Zero;
        }
    }

    void Start()
    {
        bool result = InitHook();
        Debug.Log(result);
        for(int i = 0; i < keyStates.Length; i++)
        {
            keyStates[i] = false;
        }
    }

    private void OnDestroy()
    {
        //DestroyHook();
    }

    private void OnDisable()
    {
        DestroyHook();
        Debug.Log("disabled");
    }

    private void OnEnable()
    {
        //InitHook(LowLevelKeyboardProc);
    }

    [AOT.MonoPInvokeCallback(typeof(HookProc))]
    private static int LowLevelKeyboardProc(int code, IntPtr wParam, IntPtr lParam)
    {
        if (code < 0)
        {
            return CallNextHookEx(hHook, code, wParam, lParam);
        }

        KBDLLHOOKSTRUCT hookStruct = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

        

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
        

        return CallNextHookEx(hHook, 0, wParam, lParam);

    }
}
