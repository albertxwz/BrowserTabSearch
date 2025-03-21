// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using Community.PowerToys.Run.Plugin.BrowserTabSearch.Interfaces;
using Community.PowerToys.Run.Plugin.BrowserTabSearch.Models;

namespace Community.PowerToys.Run.Plugin.BrowserTabSearch.Managers
{
    public class ChromeManager : IBrowserManager
    {
        public List<BrowserTab> GetAllTabs()
        {
            var tabs = new List<BrowserTab>();

            AutomationElement desktop = AutomationElement.RootElement;

            foreach (AutomationElement element in desktop.FindAll(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.ClassNameProperty, "Chrome_WidgetWin_1")))
            {
                try
                {
                    var tabElements = element.FindAll(
                        TreeScope.Descendants,
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem));

                    // var tabElements = GetTabElements(element);
                    foreach (AutomationElement tabElement in tabElements)
                    {
                        tabs.Add(new BrowserTab
                        {
                            TabElement = tabElement,

                            // Title = tabElement.Current.Name,
                            WindowHandle = element.Current.NativeWindowHandle,
                            BrowserType = BrowserType.Edge,

                            // TabId = tabElement.Current.AutomationId.GetHashCode(),
                        });
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return tabs;
        }

        public void ActivateTab(BrowserTab tab)
        {
            tab.Activate();
        }

        private List<BrowserTab> _cacheTabs;

        public void RefreshCache()
        {
            _cacheTabs = null;
            _cacheTabs = GetAllTabs();
        }
    }
}
