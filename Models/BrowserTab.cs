// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using System.Windows.Forms;
using Accessibility;

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

        public static bool IsWindowMinimized(IntPtr hWnd)
        {
            WINDOWPLACEMENT placement = default(WINDOWPLACEMENT);
            placement.Length = Marshal.SizeOf(placement);
            GetWindowPlacement(hWnd, ref placement);
            return placement.ShowCmd == SWSHOWMINIMIZED;
        }

        public void Activate()
        {
            if (IsWindowMinimized(WindowHandle))
            {
                ShowWindow(WindowHandle, SWRESTORE);
            }

            SetForegroundWindow(WindowHandle);

            try
            {
                SelectionItemPattern selectionPattern = TabElement.GetCurrentPattern(
                    SelectionItemPattern.Pattern) as SelectionItemPattern;
                selectionPattern?.Select();
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex);

                // LegacyIAccessiblePattern legacyPattern = element.GetCurrentPattern<LegacyIAccessiblePattern>(LegacyIAccessiblePattern.Pattern);
            }
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static List<AutomationElement> TreeFindAll(TreeWalker walker, AutomationElement root, Func<AutomationElement, bool> condition)
        {
            List<AutomationElement> result = new();
            AutomationElement element = walker.GetFirstChild(root);

            while (element != null)
            {
                if (condition(element))
                {
                    result.Add(element);
                }

                result.AddRange(TreeFindAll(walker, element, condition));

                element = walker.GetNextSibling(element);
            }

            return result;
        }
    }
}
