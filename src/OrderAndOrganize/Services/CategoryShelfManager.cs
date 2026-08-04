using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using UnityEngine;

namespace OrderAndOrganize.Services
{
    /// <summary>
    /// Manages per-shelf category assignments, keyed by grid-snapped world position.
    /// Persists to a JSON file alongside the BepInEx config.
    /// </summary>
    public class CategoryShelfManager
    {
        private readonly ManualLogSource _log;
        private readonly CategoryMapper _categoryMapper;
        private readonly string _savePath;

        private Dictionary<string, int> _assignments = new Dictionary<string, int>();

        public int AssignmentCount => _assignments.Count;

        public CategoryShelfManager(ManualLogSource log, CategoryMapper categoryMapper, string savePath)
        {
            _log = log;
            _categoryMapper = categoryMapper;
            _savePath = savePath;
            Load();
        }

        public void SetCategory(Data_Container container, int groupIndex)
        {
            string key = PositionKey(container.transform.position);
            _assignments[key] = groupIndex;
            _log?.LogInfo($"Category set: shelf at {key} -> group {groupIndex} ({_categoryMapper.GetGroupName(groupIndex)})");
            Save();
        }

        public void ClearCategory(Data_Container container)
        {
            string key = PositionKey(container.transform.position);
            if (_assignments.Remove(key))
            {
                _log?.LogInfo($"Category cleared: shelf at {key}");
                Save();
            }
        }

        /// <summary>
        /// Removes a category assignment by world position. Used when a shelf is destroyed.
        /// </summary>
        public void ClearCategoryByPosition(Vector3 position)
        {
            string key = PositionKey(position);
            if (_assignments.Remove(key))
            {
                _log?.LogInfo($"Category auto-cleared: shelf destroyed at {key}");
                Save();
            }
        }

        /// <summary>
        /// Removes assignments whose storage shelf no longer exists in the scene.
        /// Uses Physics.OverlapSphere to detect if a Data_Container is still present.
        /// Aborts if more than 50% of entries would be removed (safety against scene transitions).
        /// </summary>
        public void PurgeStaleAssignments()
        {
            if (_assignments.Count == 0) return;

            var keysToRemove = new List<string>();
            foreach (var key in _assignments.Keys)
            {
                Vector3 worldPos = ParsePositionKey(key);
                bool found = false;

                Collider[] hits = Physics.OverlapSphere(worldPos, 0.5f);
                foreach (var hit in hits)
                {
                    var dc = hit.GetComponent<Data_Container>();
                    if (dc == null)
                        dc = hit.GetComponentInParent<Data_Container>();
                    if (dc != null && dc.containerClass == 69)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    keysToRemove.Add(key);
            }

            if (keysToRemove.Count == 0) return;

            int total = _assignments.Count;
            float ratio = (float)keysToRemove.Count / total;
            if (ratio > 0.5f)
            {
                _log?.LogWarning($"CategoryShelfManager: Purge ABORTED — {keysToRemove.Count}/{total} entries " +
                    $"({ratio:P0}) would be removed. This likely means shelves haven't loaded yet.");
                return;
            }

            CreateBackup();

            foreach (var key in keysToRemove)
            {
                _assignments.Remove(key);
                _log?.LogInfo($"CategoryShelfManager: Purged stale assignment at {key} (shelf no longer exists)");
            }
            Save();
        }

        /// <summary>
        /// Returns the assigned group index for this container, or null if none.
        /// </summary>
        public int? GetCategory(Data_Container container)
        {
            string key = PositionKey(container.transform.position);
            return _assignments.TryGetValue(key, out int group) ? group : (int?)null;
        }

        /// <summary>
        /// Returns true if the product is allowed on this container (no tag = anything allowed).
        /// </summary>
        public bool IsProductAllowed(Data_Container container, int productId)
        {
            int? category = GetCategory(container);
            if (!category.HasValue)
                return true;

            int productGroup = _categoryMapper.GetGroupForProduct(productId);
            return productGroup == category.Value;
        }

        /// <summary>
        /// Returns all assigned containers' position keys and their group indices.
        /// Used for rendering floating labels.
        /// </summary>
        public IReadOnlyDictionary<string, int> GetAllAssignments()
        {
            return _assignments;
        }

        /// <summary>
        /// Checks if a container at the given position has a category that matches the product.
        /// Used by employee patches where we only have transform position.
        /// </summary>
        public bool IsProductAllowedAtPosition(Vector3 position, int productId)
        {
            string key = PositionKey(position);
            if (!_assignments.TryGetValue(key, out int group))
                return true;

            int productGroup = _categoryMapper.GetGroupForProduct(productId);
            return productGroup == group;
        }

        /// <summary>
        /// Returns the category at a world position, or null.
        /// </summary>
        public int? GetCategoryAtPosition(Vector3 position)
        {
            string key = PositionKey(position);
            return _assignments.TryGetValue(key, out int group) ? group : (int?)null;
        }

        /// <summary>
        /// Produces a stable, grid-snapped key from a world position.
        /// Rounded to 1 decimal place (0.1 unit precision) to avoid float drift.
        /// </summary>
        public static string PositionKey(Vector3 pos)
        {
            int x = Mathf.RoundToInt(pos.x * 10f);
            int y = Mathf.RoundToInt(pos.y * 10f);
            int z = Mathf.RoundToInt(pos.z * 10f);
            return $"{x},{y},{z}";
        }

        /// <summary>
        /// Parses a position key back to approximate world position (for label rendering).
        /// </summary>
        public static Vector3 ParsePositionKey(string key)
        {
            var parts = key.Split(',');
            if (parts.Length != 3) return Vector3.zero;
            if (!int.TryParse(parts[0], out int x)) return Vector3.zero;
            if (!int.TryParse(parts[1], out int y)) return Vector3.zero;
            if (!int.TryParse(parts[2], out int z)) return Vector3.zero;
            return new Vector3(x / 10f, y / 10f, z / 10f);
        }

        private string BackupPath => _savePath + ".backup";

        /// <summary>
        /// Creates a backup copy of the current save file before destructive operations.
        /// Overwrites the previous backup (single rotating backup).
        /// </summary>
        private void CreateBackup()
        {
            try
            {
                if (File.Exists(_savePath))
                {
                    File.Copy(_savePath, BackupPath, overwrite: true);
                    _log?.LogInfo($"CategoryShelfManager: Backup created at {BackupPath}");
                }
            }
            catch (Exception ex)
            {
                _log?.LogWarning($"CategoryShelfManager: Failed to create backup: {ex.Message}");
            }
        }

        /// <summary>
        /// Attempts to restore assignments from the backup file.
        /// Returns true if assignments were restored.
        /// </summary>
        private bool TryRestoreFromBackup()
        {
            if (!File.Exists(BackupPath)) return false;

            try
            {
                string json = File.ReadAllText(BackupPath);
                var restored = new Dictionary<string, int>();
                var temp = _assignments;
                _assignments = restored;
                ParseJson(json);
                int restoredCount = _assignments.Count;
                _assignments = temp;

                if (restoredCount == 0) return false;

                _assignments = restored;
                _log?.LogWarning($"CategoryShelfManager: Restored {restoredCount} assignments from backup.");
                Save();
                return true;
            }
            catch (Exception ex)
            {
                _log?.LogWarning($"CategoryShelfManager: Failed to restore from backup: {ex.Message}");
                return false;
            }
        }

        private void Save()
        {
            try
            {
                string dir = Path.GetDirectoryName(_savePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("{");
                sb.AppendLine("  \"version\": 1,");
                sb.AppendLine("  \"assignments\": {");

                int count = 0;
                foreach (var kv in _assignments)
                {
                    count++;
                    string comma = count < _assignments.Count ? "," : "";
                    sb.AppendLine($"    \"{EscapeJson(kv.Key)}\": {kv.Value}{comma}");
                }

                sb.AppendLine("  }");
                sb.AppendLine("}");

                File.WriteAllText(_savePath, sb.ToString());
                _log?.LogDebug($"CategoryShelfManager: Saved {_assignments.Count} assignments to {_savePath}");
            }
            catch (Exception ex)
            {
                _log?.LogError($"CategoryShelfManager: Failed to save: {ex.Message}");
            }
        }

        private void Load()
        {
            _assignments.Clear();

            if (!File.Exists(_savePath))
            {
                _log?.LogDebug("CategoryShelfManager: No save file found, starting fresh.");
                return;
            }

            try
            {
                string json = File.ReadAllText(_savePath);
                ParseJson(json);
                PurgeCorruptKeys();
                _log?.LogInfo($"CategoryShelfManager: Loaded {_assignments.Count} assignments from {_savePath}");
            }
            catch (Exception ex)
            {
                _log?.LogError($"CategoryShelfManager: Failed to load: {ex.Message}");
            }

            if (_assignments.Count == 0 && File.Exists(BackupPath))
            {
                _log?.LogWarning("CategoryShelfManager: Main file has 0 assignments but backup exists — attempting restore.");
                TryRestoreFromBackup();
            }
        }

        private void ParseJson(string json)
        {
            int assignIdx = json.IndexOf("\"assignments\"", StringComparison.Ordinal);
            if (assignIdx < 0) return;

            int braceStart = json.IndexOf('{', assignIdx + 13);
            if (braceStart < 0) return;

            int braceEnd = json.IndexOf('}', braceStart);
            if (braceEnd < 0) return;

            string inner = json.Substring(braceStart + 1, braceEnd - braceStart - 1);

            // Line-based parsing: each entry is on its own line as `"key": value,`
            // Keys contain commas (position format "x,y,z") so we cannot split on commas.
            var lines = inner.Split('\n');
            foreach (string line in lines)
            {
                string trimmed = line.Trim().TrimEnd(',');
                if (string.IsNullOrEmpty(trimmed)) continue;

                int firstQuote = trimmed.IndexOf('"');
                if (firstQuote < 0) continue;
                int lastQuote = trimmed.LastIndexOf('"');
                if (lastQuote <= firstQuote) continue;

                string key = trimmed.Substring(firstQuote + 1, lastQuote - firstQuote - 1);
                string valStr = trimmed.Substring(trimmed.LastIndexOf(':') + 1).Trim();

                if (!string.IsNullOrEmpty(key) && int.TryParse(valStr, out int groupIndex))
                {
                    _assignments[key] = groupIndex;
                }
            }
        }

        private void PurgeCorruptKeys()
        {
            var keysToRemove = new List<string>();
            foreach (var key in _assignments.Keys)
            {
                var parts = key.Split(',');
                if (parts.Length != 3
                    || !int.TryParse(parts[0], out _)
                    || !int.TryParse(parts[1], out _)
                    || !int.TryParse(parts[2], out _))
                {
                    keysToRemove.Add(key);
                }
            }

            if (keysToRemove.Count > 0)
            {
                foreach (var bad in keysToRemove)
                {
                    _assignments.Remove(bad);
                    _log?.LogWarning($"CategoryShelfManager: Removed corrupt key: '{bad}'");
                }
                Save();
            }
        }

        private static string EscapeJson(string s)
        {
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
