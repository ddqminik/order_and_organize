using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;

namespace OrderAndOrganize.Services
{
    /// <summary>
    /// Maps product IDs to their ordering-tab group index using
    /// ProductListing.tiers (tier ranges) and ProductListing.productGroups.
    /// </summary>
    public class CategoryMapper
    {
        private readonly ManualLogSource _log;
        private Dictionary<int, int> _productToGroup;
        private Dictionary<int, string> _groupNames;
        private Dictionary<int, Color> _groupColors;
        private bool _initialized;

        public CategoryMapper(ManualLogSource log)
        {
            _log = log;
        }

        /// <summary>
        /// Returns the ordering-tab group index for a given product ID, or -1 if unknown.
        /// </summary>
        public int GetGroupForProduct(int productId)
        {
            EnsureInitialized();
            return _productToGroup.TryGetValue(productId, out int group) ? group : -1;
        }

        /// <summary>
        /// Returns the localized name for a group index, or a fallback string.
        /// </summary>
        public string GetGroupName(int groupIndex)
        {
            EnsureInitialized();
            if (_groupNames.TryGetValue(groupIndex, out string name))
                return name;
            return $"Group {groupIndex}";
        }

        /// <summary>
        /// Returns the native game color for a group index, or white as fallback.
        /// </summary>
        public Color GetGroupColor(int groupIndex)
        {
            EnsureInitialized();
            return _groupColors.TryGetValue(groupIndex, out Color c) ? c : Color.white;
        }

        /// <summary>
        /// Returns all known distinct group indices and their names.
        /// </summary>
        public Dictionary<int, string> GetAllGroups()
        {
            EnsureInitialized();
            return new Dictionary<int, string>(_groupNames);
        }

        /// <summary>
        /// Forces a rebuild of the mapping cache. Call after ProductListing is fully loaded.
        /// </summary>
        public void Rebuild()
        {
            _initialized = false;
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;

            _productToGroup = new Dictionary<int, int>();
            _groupNames = new Dictionary<int, string>();
            _groupColors = new Dictionary<int, Color>();

            var listing = ProductListing.Instance;
            if (listing == null || listing.tiers == null || listing.productGroups == null)
            {
                _log?.LogWarning("CategoryMapper: ProductListing not available yet.");
                return;
            }

            _initialized = true;

            for (int tierIndex = 0; tierIndex < listing.tiers.Length; tierIndex++)
            {
                string tierRange = listing.tiers[tierIndex];
                if (string.IsNullOrEmpty(tierRange)) continue;

                var parts = tierRange.Split('-');
                if (parts.Length != 2) continue;

                if (!int.TryParse(parts[0], out int start) || !int.TryParse(parts[1], out int end))
                    continue;

                int groupIndex = tierIndex < listing.productGroups.Length
                    ? listing.productGroups[tierIndex]
                    : -1;

                if (groupIndex < 0) continue;

                for (int pid = start; pid <= end; pid++)
                {
                    _productToGroup[pid] = groupIndex;
                }

                if (!_groupNames.ContainsKey(groupIndex))
                {
                    string locKey = "productGroup" + groupIndex;
                    string name = null;
                    try
                    {
                        if (LocalizationManager.instance != null)
                            name = LocalizationManager.instance.GetLocalizationString(locKey);
                    }
                    catch { }

                    _groupNames[groupIndex] = !string.IsNullOrEmpty(name) ? name : $"Group {groupIndex}";

                    if (listing.groupsColors != null && groupIndex < listing.groupsColors.Length)
                        _groupColors[groupIndex] = listing.groupsColors[groupIndex];
                    else
                        _groupColors[groupIndex] = Color.white;
                }
            }

            _log?.LogInfo($"CategoryMapper: Mapped {_productToGroup.Count} products across {_groupNames.Count} groups.");
            foreach (var kv in _groupNames)
            {
                _log?.LogDebug($"  Group {kv.Key}: {kv.Value}");
            }
        }
    }
}
