// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using Community.PowerToys.Run.Plugin.BrowserTabSearch.Interfaces;
using Community.PowerToys.Run.Plugin.BrowserTabSearch.Managers;
using Community.PowerToys.Run.Plugin.BrowserTabSearch.Models;
using Community.PowerToys.Run.Plugin.BrowserTabSearch.Properties;
using hyjiacan.py4n;
using ManagedCommon;
using Microsoft.PowerToys.Settings.UI.Library;
using Wox.Plugin;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.BrowserTabSearch
{
    public class Main : IPlugin, IPluginI18n, IContextMenu, ISettingProvider, IReloadable, IDisposable
    {
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
            ArgumentNullException.ThrowIfNull(query);

            var results = new List<Result>();

            // empty query
            if (string.IsNullOrEmpty(query.Search))
            {
                // RefreshAllTabs();
                _results = GetAllTabs();
                Log.Info($"Refresh tabs {_results.Count}.", GetType());
                Debug.WriteLine($"Refresh tabs {_results.Count}.");

                if (_maxResults >= 0)
                {
                    results.AddRange(_results.Take(_maxResults));
                }
                else
                {
                    results.AddRange(_results);
                }

                return results;
            }

            foreach (var r in _results)
            {
                // int score = StringMatcher.FuzzySearch(query.Search, r.Title).Score;
                int score = CalculateScore(query.Search, r.Title);
                if (score <= 0)
                {
                    continue;
                }

                r.Score = score;
                results.Add(r);
            }

            results.Sort((x, y) => y.Score.CompareTo(x.Score));

            return _maxResults >= 0 ? results.Take(_maxResults).ToList() : results.ToList();
        }

        private static bool ContainsChineseCharacters(string text)
        {
            return Regex.IsMatch(text, @"[\u4e00-\u9fa5]");
        }

        private static string ConvertChineseText(string text)
        {
            StringBuilder result = new();
            foreach (char ch in text)
            {
                if (ch >= 0x4e00 && ch <= 0x9fa5)
                {
                    result.Append(Pinyin4Net.GetFirstPinyin(ch, PinyinFormat.WITHOUT_TONE));
                }
                else
                {
                    result.Append(ch);
                }
            }

            return result.ToString();
        }

        private static int CalculateScore(string query, string title)
        {
            float titleScore = title.Contains(query, StringComparison.InvariantCultureIgnoreCase)
                ? (query.Length / (float)title.Length * 100f)
                : 0;

            if (titleScore <= 0.0 && ContainsChineseCharacters(title) && !ContainsChineseCharacters(query))
            {
                string pinyinTitle = ConvertChineseText(title);
                titleScore = pinyinTitle.Contains(query, StringComparison.InvariantCultureIgnoreCase)
                    ? (query.Length / (float)pinyinTitle.Length * 100f)
                    : 0;
            }

            return (int)titleScore;
        }

        private List<IBrowserManager> _managers;
        private List<Result> _results = new();

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

            Log.Info("Initialized BrowserTabSearch", GetType());

            // Task.Run(UpdateCacheLoop);
        }

        private List<Result> GetAllTabs()
        {
            List<Result> results = new();
            foreach (var manager in _managers)
            {
                if (manager != null)
                {
                    var tabs = manager.GetAllTabs();
                    foreach (var tab in tabs)
                    {
                        results.Add(new Result
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

            return results;
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

                _disposed = true;
            }
        }
    }
}
