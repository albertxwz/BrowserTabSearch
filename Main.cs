// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Community.PowerToys.Run.Plugin.BrowserTabSearch.Interfaces;
using Community.PowerToys.Run.Plugin.BrowserTabSearch.Managers;
using Community.PowerToys.Run.Plugin.BrowserTabSearch.Models;
using Community.PowerToys.Run.Plugin.BrowserTabSearch.Properties;
using ManagedCommon;
using Microsoft.International.Converters.PinYinConverter;
using Microsoft.PowerToys.Settings.UI.Library;
using Wox.Infrastructure;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.BrowserTabSearch
{
    public class Main : IPlugin, IPluginI18n, IContextMenu, ISettingProvider, IReloadable, IDisposable, IDelayedExecutionPlugin
    {
        private CancellationTokenSource _cancellationTokenSource = new();

        // private const string Setting = nameof(Setting);

        // current value of the setting
        // private bool _setting;
        private PluginInitContext _context;
        private string _iconPath;
        private bool _disposed;

        public string Name => Resources.plugin_name;

        public string Description => Resources.plugin_description;

        // TODO: remove dash from ID below and inside plugin.json
        public static string PluginID => "71bc0779-fae8-4f18-bfac-7a716fd910cf";

        // TODO: add additional options (optional)
        public IEnumerable<PluginAdditionalOption> AdditionalOptions => new List<PluginAdditionalOption>()
        {
            new()
            {
                Key = MaxResults,
                DisplayLabel = "Maximum number of results",
                DisplayDescription = "Maximum number of results to show. Set to -1 to show all (may decrease performance)",
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Numberbox,
                NumberValue = -1,
            },
        };

        private const string MaxResults = nameof(MaxResults);
        private int _maxResults;

        public void UpdateSettings(PowerLauncherPluginSettings settings)
        {
            // _setting = settings?.AdditionalOptions?.FirstOrDefault(x => x.Key == Setting)?.Value ?? false;
            _maxResults = (int)(settings?.AdditionalOptions?.FirstOrDefault(x => x.Key == MaxResults)?.NumberValue ?? -1);
        }

        // TODO: return context menus for each Result (optional)
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            return new List<ContextMenuResult>(0);
        }

        // TODO: return query results
        public List<Result> Query(Query query)
        {
            return null;
        }

        public List<Result> Query(Query query, bool delayedExecution)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            ArgumentNullException.ThrowIfNull(query);

            Debug.WriteLine(query.Search);

            // empty query
            if (string.IsNullOrEmpty(query.Search))
            {
                return _maxResults >= 0 ? _results.Take(_maxResults).ToList() : _results;
            }

            var results = new List<Result>();

            foreach (var r in _results)
            {
                // int score = CalculateScore(query.Search, r.Title, r.SubTitle);
                int score = StringMatcher.FuzzySearch(query.Search, r.Title).Score;
                if (score <= 0)
                {
                    continue;
                }

                r.Score = score;
                results.Add(r);
            }

            results.Sort((x, y) => y.Score.CompareTo(x.Score));

            return _maxResults >= 0 ? results.Take(_maxResults).ToList() : _results;
        }

        private List<IBrowserManager> _managers;
        private List<Result> _results;

        public void Init(PluginInitContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(_context.API.GetCurrentTheme());
            _managers = new List<IBrowserManager>();
            foreach (BrowserType browserType in Enum.GetValues(typeof(BrowserType)))
            {
                _managers.Add(BrowserManagerFactory.CreateManager(browserType));
            }

            Task.Run(UpdateCacheLoop);
        }

        private void RefreshAllTabs()
        {
            _results = new List<Result>();
            foreach (var manager in _managers)
            {
                if (manager != null)
                {
                    var tabs = manager.GetAllTabs();
                    foreach (var tab in tabs)
                    {
                        _results.Add(new Result
                        {
                            Title = tab.TabElement.Current.Name,
                            SubTitle = string.Empty,

                            // SubTitle = tab.Url,
                            IcoPath = _iconPath,
                            Action = action =>
                            {
                                tab.Activate();
                                return true;
                            },
                        });
                    }
                }
            }

            Debug.WriteLine(_results.Count);
        }

        public async Task UpdateCacheLoop()
        {
            while (true)
            {
                RefreshAllTabs();
                await Task.Delay(1000);
            }
        }

        public string GetTranslatedPluginTitle()
        {
            return Resources.plugin_name;
        }

        public string GetTranslatedPluginDescription()
        {
            return Resources.plugin_description;
        }

        private void OnThemeChanged(Theme oldTheme, Theme newTheme)
        {
            UpdateIconPath(newTheme);
        }

        private void UpdateIconPath(Theme theme)
        {
            if (theme == Theme.Light || theme == Theme.HighContrastWhite)
            {
                _iconPath = "Images/BrowserTabSearch.light.png";
            }
            else
            {
                _iconPath = "Images/BrowserTabSearch.dark.png";
            }
        }

        public Control CreateSettingPanel()
        {
            throw new NotImplementedException();
        }

        public void ReloadData()
        {
            if (_context is null)
            {
                return;
            }

            UpdateIconPath(_context.API.GetCurrentTheme());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (_context != null && _context.API != null)
                {
                    _context.API.ThemeChanged -= OnThemeChanged;
                }

                _cancellationTokenSource?.Dispose();
                _disposed = true;
            }
        }
    }
}
