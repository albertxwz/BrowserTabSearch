// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Automation;
using Community.PowerToys.Run.Plugin.BrowserTabSearch.Interfaces;
using Community.PowerToys.Run.Plugin.BrowserTabSearch.Models;

namespace Community.PowerToys.Run.Plugin.BrowserTabSearch.Managers
{
    public class EdgeManager : IBrowserManager
    {
        public List<BrowserTab> GetAllTabs()
        {
            var tabs = new List<BrowserTab>();
            var processes = Process.GetProcessesByName("msedge");

            foreach (var process in processes)
            {
                try
                {
                    var element = AutomationElement.FromHandle(process.MainWindowHandle);
                    if (element == null)
                    {
                        continue;
                    }

                    var tabElements = element.FindAll(
                        TreeScope.Descendants,
                        new AndCondition(
                            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem),
                            new PropertyCondition(AutomationElement.ClassNameProperty, "EdgeTab")));

                    foreach (AutomationElement tabElement in tabElements)
                    {
                        tabs.Add(new BrowserTab
                        {
                            TabElement = tabElement,

                            // Title = tabElement.Current.Name,
                            WindowHandle = process.MainWindowHandle,
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
