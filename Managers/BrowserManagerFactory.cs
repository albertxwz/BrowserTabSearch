// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Community.PowerToys.Run.Plugin.BrowserTabSearch.Interfaces;
using Community.PowerToys.Run.Plugin.BrowserTabSearch.Models;

namespace Community.PowerToys.Run.Plugin.BrowserTabSearch.Managers
{
    public class BrowserManagerFactory
    {
        public static IBrowserManager CreateManager(BrowserType type)
        {
            return type switch
            {
                BrowserType.Edge => new EdgeManager(),
                _ => null,
            };
        }
    }
}
