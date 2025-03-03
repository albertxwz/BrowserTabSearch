// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using System.Windows.Automation;

namespace Community.PowerToys.Run.Plugin.BrowserTabSearch.Models
{
    public class BrowserTab
    {
        public AutomationElement TabElement { get; set; }

        // public string Title { get; set; }

        // public string Url { get; set; }
        public IntPtr WindowHandle { get; set; }

        // public int TabId { get; set; }
        public BrowserType BrowserType { get; set; }

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        // Define window commands for ShowWindow
        private const int SWRESTORE = 9; // Restores a minimized window
        private const int SWSHOWMINIMIZED = 2; // Minimized state

        // Define WINDOWPLACEMENT structure
        private struct WINDOWPLACEMENT
        {
            public int Length;
            public int Flags;
            public int ShowCmd;
            public System.Drawing.Point PtMinPosition;
            public System.Drawing.Point PtMaxPosition;
            public System.Drawing.Rectangle RcNormalPosition;
        }

        public void Activate()
        {
            WINDOWPLACEMENT placement = default(WINDOWPLACEMENT);
            placement.Length = Marshal.SizeOf(placement);
            GetWindowPlacement(WindowHandle, ref placement);

            if (placement.ShowCmd == SWSHOWMINIMIZED)
            {
                ShowWindow(WindowHandle, SWRESTORE);
            }

            SetForegroundWindow(WindowHandle);

            SelectionItemPattern selectionPattern = TabElement.GetCurrentPattern(
            SelectionItemPattern.Pattern) as SelectionItemPattern;
            if (selectionPattern != null)
            {
                selectionPattern.Select();
                return;
            }

            // InvokePattern invokePattern = TabElement.GetCurrentPattern(InvokePattern.Pattern)
            //    as InvokePattern;

            // if (invokePattern != null)
            // {
            //    invokePattern.Invoke();
            // }
            // else
            // {
            //    // Fallback: Use SelectionItemPattern if the tab is part of a selection group
            //    SelectionItemPattern selectionPattern = TabElement.GetCurrentPattern(SelectionItemPattern.Pattern)
            //        as SelectionItemPattern;
            //    selectionPattern?.Select();
            // }
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
