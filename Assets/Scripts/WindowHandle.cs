using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class WindowHandle : MonoBehaviour
{
    // Import the function to get the current active window
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    // Import the function to change the window properties
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    // we use this to put our application at the front
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    // definitions for property flags
    const int GWL_EXSTYLE = -20;
    const uint WS_EX_LAYERED = 0x00080000;
    const uint WS_EX_TRANSPARENT = 0x00000020;
    // window pointer
    private IntPtr hWnd;

    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

    // Start is called before the first frame update
    void Start()
    {
        // obviously we don't want to change the unity editor, so only do in standalone
#if !UNITY_EDITOR
        // get the active window
        hWnd = GetActiveWindow();
        // get the margins
        MARGINS margins = new MARGINS { cxLeftWidth = -1 };

        DwmExtendFrameIntoClientArea(hWnd, ref margins);
        // update the window properties
        SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT);
        // move it to the front
        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, 0);
#endif
    }

    // Update is called once per frame
    void Update()
    {
        // handle clicks whether or not we clickthrough the application or onto the application
        SetClickthrough(Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition)) == null);
    }

    void SetClickthrough(bool clickthrough)
    {
        if(clickthrough)
        {
            // send click to the application behind
            SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT);
        }
        else
        {
            // handle click ourselves
            SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);
        }
    }
}
