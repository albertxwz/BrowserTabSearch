// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Community.PowerToys.Run.Plugin.BrowserTabSearch.Models;

namespace Community.PowerToys.Run.Plugin.BrowserTabSearch.Interfaces
{
    public interface IBrowserManager
    {
        public List<BrowserTab> GetAllTabs();

        public void ActivateTab(BrowserTab tab);

        public void RefreshCache();
    }
}
