// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Community.PowerToys.Run.Plugin.BrowserTabSearch.Models;
using Microsoft.PowerToys.Settings.UI.Library;

namespace Community.PowerToys.Run.Plugin.BrowserTabSearch.Interfaces
{
    public interface IBrowserManager
    {
        public List<BrowserTab> GetAllTabs();

        public void ActivateTab(BrowserTab tab);

        public void RefreshCache();
    }
}
