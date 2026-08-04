using System;
using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using OrderAndOrganize.Services;
using UnityEngine;

namespace OrderAndOrganize.UI
{
    /// <summary>
    /// IMGUI-based scroll-wheel category picker for storage shelves, plus
    /// toggleable floating labels showing assigned categories above tagged shelves.
    /// </summary>
    public class CategoryPickerUI
    {
        private readonly ManualLogSource _log;
        private readonly CategoryShelfManager _shelfManager;
        private readonly CategoryMapper _categoryMapper;

        private bool _pickerVisible;
        private Data_Container _targetContainer;

        private List<KeyValuePair<int, string>> _sortedGroups;
        private int _selectedIndex;

        private CursorLockMode _savedLockState;
        private bool _savedCursorVisible;
        private MonoBehaviour _frozenController;

        private static Type _fpcType;
        private static bool _fpcTypeResolved;
        private static MonoBehaviour _cachedFpc;

        private bool _labelsVisible = true;
        private bool _skipNextFrame;

        private GUIStyle _panelBgStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _itemStyle;
        private GUIStyle _selectedItemStyle;
        private GUIStyle _footerStyle;
        private bool _stylesInitialized;

        private const float PanelWidth = 320f;
        private const float PanelHeight = 360f;
        private const float ItemHeight = 30f;
        private const int VisibleItemCount = 7;
        private const float LabelMaxDistance = 20f;

        public bool IsPickerVisible => _pickerVisible;
        public bool LabelsVisible
        {
            get => _labelsVisible;
            set => _labelsVisible = value;
        }

        public CategoryPickerUI(ManualLogSource log, CategoryShelfManager shelfManager, CategoryMapper categoryMapper)
        {
            _log = log;
            _shelfManager = shelfManager;
            _categoryMapper = categoryMapper;
        }

        /// <summary>
        /// Called from Plugin.Update() every frame. Blocks all game input while
        /// the picker is open by resetting input axes.
        /// </summary>
        /// <summary>
        /// Sole input handler for the picker, called from Plugin.Update().
        /// All key/scroll detection happens here BEFORE ResetInputAxes clears input.
        /// IMGUI Draw() is purely visual -- no input handling there.
        /// </summary>
        public void UpdateInput()
        {
            if (!_pickerVisible) return;

            // Skip the frame the picker was opened to avoid the same G press closing it
            if (_skipNextFrame)
            {
                _skipNextFrame = false;
                Input.ResetInputAxes();
                return;
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                ClosePicker(apply: false);
                Input.ResetInputAxes();
                return;
            }
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                ClosePicker(apply: true);
                Input.ResetInputAxes();
                return;
            }
            if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete))
            {
                ClearAndClose();
                Input.ResetInputAxes();
                return;
            }

            if (_sortedGroups != null)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (scroll > 0.01f)
                    _selectedIndex = (_selectedIndex - 1 + _sortedGroups.Count) % _sortedGroups.Count;
                else if (scroll < -0.01f)
                    _selectedIndex = (_selectedIndex + 1) % _sortedGroups.Count;

                if (Input.GetKeyDown(KeyCode.UpArrow))
                    _selectedIndex = (_selectedIndex - 1 + _sortedGroups.Count) % _sortedGroups.Count;
                if (Input.GetKeyDown(KeyCode.DownArrow))
                    _selectedIndex = (_selectedIndex + 1) % _sortedGroups.Count;
            }

            Input.ResetInputAxes();
        }

        public void OnHotkeyPressed()
        {
            if (_pickerVisible)
            {
                ClosePicker(apply: false);
                return;
            }

            Data_Container container = RaycastForStorageShelf();
            if (container == null)
            {
                _log?.LogDebug("CategoryPicker: No storage shelf found in crosshair.");
                return;
            }

            OpenPicker(container);
        }

        public void ToggleLabels()
        {
            _labelsVisible = !_labelsVisible;
            try
            {
                string state = _labelsVisible ? "ON" : "OFF";
                GameCanvas.Instance?.CreateCanvasNotification($"`Category labels: {state}");
            }
            catch { }
        }

        private void OpenPicker(Data_Container container)
        {
            _targetContainer = container;

            _sortedGroups = new List<KeyValuePair<int, string>>(_categoryMapper.GetAllGroups());
            _sortedGroups.Sort((a, b) => a.Key.CompareTo(b.Key));

            if (_sortedGroups.Count == 0)
            {
                _log?.LogWarning("CategoryPicker: No category groups available.");
                return;
            }

            int? currentCategory = _shelfManager.GetCategory(container);
            _selectedIndex = 0;
            if (currentCategory.HasValue)
            {
                for (int i = 0; i < _sortedGroups.Count; i++)
                {
                    if (_sortedGroups[i].Key == currentCategory.Value)
                    {
                        _selectedIndex = i;
                        break;
                    }
                }
            }

            _pickerVisible = true;
            _skipNextFrame = true;

            _savedLockState = Cursor.lockState;
            _savedCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            FreezePlayerController();

            _log?.LogDebug($"CategoryPicker: Opened for shelf at {container.transform.position}");
        }

        private void ClosePicker(bool apply)
        {
            if (apply && _targetContainer != null && _sortedGroups != null
                && _selectedIndex >= 0 && _selectedIndex < _sortedGroups.Count)
            {
                var selected = _sortedGroups[_selectedIndex];
                _shelfManager.SetCategory(_targetContainer, selected.Key);
                try
                {
                    GameCanvas.Instance?.CreateCanvasNotification($"`Shelf tagged: {selected.Value}");
                }
                catch { }
            }

            _pickerVisible = false;
            _targetContainer = null;
            _sortedGroups = null;

            UnfreezePlayerController();

            Cursor.lockState = _savedLockState;
            Cursor.visible = _savedCursorVisible;
        }

        private void ClearAndClose()
        {
            if (_targetContainer != null)
            {
                _shelfManager.ClearCategory(_targetContainer);
                try
                {
                    GameCanvas.Instance?.CreateCanvasNotification("`Shelf category cleared");
                }
                catch { }
            }

            _pickerVisible = false;
            _targetContainer = null;
            _sortedGroups = null;

            UnfreezePlayerController();

            Cursor.lockState = _savedLockState;
            Cursor.visible = _savedCursorVisible;
        }

        public void Draw()
        {
            InitStyles();

            if (_labelsVisible)
                DrawFloatingLabels();

            if (!_pickerVisible || _sortedGroups == null || _sortedGroups.Count == 0)
                return;

            DrawPickerOverlay();
        }

        private void DrawPickerOverlay()
        {
            float panelX = (Screen.width - PanelWidth) / 2f;
            float panelY = (Screen.height - PanelHeight) / 2f;
            Rect panelRect = new Rect(panelX, panelY, PanelWidth, PanelHeight);

            GUI.Box(panelRect, "", _panelBgStyle);

            GUILayout.BeginArea(new Rect(panelX + 12f, panelY + 10f, PanelWidth - 24f, PanelHeight - 20f));

            GUILayout.Label("Assign Category", _titleStyle);
            GUILayout.Space(4f);

            int? currentCategory = _shelfManager.GetCategory(_targetContainer);
            string currentName = currentCategory.HasValue
                ? _categoryMapper.GetGroupName(currentCategory.Value) : "None";

            var currentStyle = new GUIStyle(_footerStyle) { alignment = TextAnchor.MiddleCenter };
            currentStyle.normal.textColor = currentCategory.HasValue
                ? _categoryMapper.GetGroupColor(currentCategory.Value)
                : new Color(0.6f, 0.6f, 0.6f);
            GUILayout.Label($"Current: {currentName}", currentStyle);

            GUILayout.Space(6f);

            DrawSeparator(PanelWidth - 24f);
            GUILayout.Space(6f);

            int halfVisible = VisibleItemCount / 2;
            int startIdx = _selectedIndex - halfVisible;

            for (int v = 0; v < VisibleItemCount; v++)
            {
                int idx = startIdx + v;
                // Wrap around
                idx = ((idx % _sortedGroups.Count) + _sortedGroups.Count) % _sortedGroups.Count;

                var kv = _sortedGroups[idx];
                bool isSelected = (idx == _selectedIndex);
                Color catColor = EnsureReadable(_categoryMapper.GetGroupColor(kv.Key));

                if (isSelected)
                {
                    Rect itemRect = GUILayoutUtility.GetRect(PanelWidth - 24f, ItemHeight);

                    Color bgColor = catColor * 0.4f;
                    bgColor.a = 0.9f;
                    GUI.DrawTexture(itemRect, MakeTex(1, 1, bgColor));

                    var selStyle = new GUIStyle(_selectedItemStyle);
                    selStyle.normal.textColor = Color.white;
                    GUI.Label(itemRect, $"  > {kv.Value}", selStyle);
                }
                else
                {
                    Rect itemRect = GUILayoutUtility.GetRect(PanelWidth - 24f, ItemHeight);

                    float distFromCenter = Mathf.Abs(v - halfVisible);
                    float alpha = Mathf.Lerp(1f, 0.45f, distFromCenter / (halfVisible + 1f));

                    var dimStyle = new GUIStyle(_itemStyle);
                    Color dimColor = catColor;
                    dimColor.a = alpha;
                    dimStyle.normal.textColor = dimColor;
                    GUI.Label(itemRect, $"    {kv.Value}", dimStyle);
                }
            }

            GUILayout.Space(6f);
            DrawSeparator(PanelWidth - 24f);
            GUILayout.Space(8f);

            GUILayout.Label("[Scroll/Arrows] Browse   [Enter] Apply", _footerStyle);
            GUILayout.Label("[G] Cancel   [Backspace] Clear", _footerStyle);

            GUILayout.EndArea();
        }

        private void DrawFloatingLabels()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            var assignments = _shelfManager.GetAllAssignments();
            if (assignments.Count == 0) return;

            Vector3 camPos = cam.transform.position;

            foreach (var kv in assignments)
            {
                Vector3 worldPos = CategoryShelfManager.ParsePositionKey(kv.Key);
                float dist = Vector3.Distance(camPos, worldPos);
                if (dist > LabelMaxDistance) continue;

                Vector3 labelWorldPos = worldPos + Vector3.up * 2.2f;
                Vector3 screenPos = cam.WorldToScreenPoint(labelWorldPos);

                if (screenPos.z <= 0f) continue;

                float screenY = Screen.height - screenPos.y;

                string groupName = _categoryMapper.GetGroupName(kv.Value);
                Color catColor = _categoryMapper.GetGroupColor(kv.Value);

                float scale = Mathf.Clamp01(1f - dist / LabelMaxDistance);
                int fontSize = Mathf.RoundToInt(Mathf.Lerp(10f, 16f, scale));

                var floatStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = fontSize,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                catColor.a = 0.7f + 0.3f * scale;
                floatStyle.normal.textColor = catColor;

                Vector2 size = floatStyle.CalcSize(new GUIContent(groupName));
                Rect labelRect = new Rect(
                    screenPos.x - size.x / 2f,
                    screenY - size.y / 2f,
                    size.x + 8f,
                    size.y + 2f);

                GUI.DrawTexture(labelRect, MakeTex(1, 1, new Color(0f, 0f, 0f, 0.4f * scale)));
                GUI.Label(labelRect, groupName, floatStyle);
            }
        }

        private Data_Container RaycastForStorageShelf()
        {
            Camera cam = Camera.main;
            if (cam == null) return null;

            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

            if (!Physics.Raycast(ray, out RaycastHit hit, 8f))
                return null;

            Transform current = hit.transform;
            int maxDepth = 6;
            while (current != null && maxDepth-- > 0)
            {
                var dc = current.GetComponent<Data_Container>();
                if (dc != null)
                {
                    if (dc.containerClass == 69)
                        return dc;

                    _log?.LogDebug($"CategoryPicker: Found Data_Container but containerClass={dc.containerClass} (not storage)");
                    return null;
                }

                var ic = current.GetComponent<InteractableContainer>();
                if (ic != null && ic.isStorageShelf)
                {
                    var parentDc = current.parent?.parent?.GetComponent<Data_Container>();
                    if (parentDc != null && parentDc.containerClass == 69)
                        return parentDc;
                }

                current = current.parent;
            }

            return null;
        }

        // --- Player controller freeze/unfreeze via FindObjectOfType ---

        private void FreezePlayerController()
        {
            try
            {
                var fpc = FindFirstPersonController();
                if (fpc != null && fpc.enabled)
                {
                    fpc.enabled = false;
                    _frozenController = fpc;
                    _log?.LogDebug("CategoryPicker: Disabled FirstPersonController.");
                }
            }
            catch (Exception ex)
            {
                _log?.LogWarning($"CategoryPicker: Failed to freeze player controller: {ex.Message}");
            }
        }

        private void UnfreezePlayerController()
        {
            try
            {
                if (_frozenController != null)
                {
                    _frozenController.enabled = true;
                    _frozenController = null;
                    _log?.LogDebug("CategoryPicker: Re-enabled FirstPersonController.");
                }
            }
            catch (Exception ex)
            {
                _log?.LogWarning($"CategoryPicker: Failed to unfreeze player controller: {ex.Message}");
            }
        }

        internal static MonoBehaviour FindFirstPersonController()
        {
            if (_cachedFpc != null) return _cachedFpc;

            if (!_fpcTypeResolved)
            {
                _fpcTypeResolved = true;
                _fpcType = AccessTools.TypeByName("StarterAssets.FirstPersonController")
                        ?? AccessTools.TypeByName("FirstPersonController");
            }

            if (_fpcType == null) return null;

            _cachedFpc = UnityEngine.Object.FindFirstObjectByType(_fpcType) as MonoBehaviour;
            return _cachedFpc;
        }

        // --- Styles ---

        private void InitStyles()
        {
            if (_stylesInitialized) return;
            _stylesInitialized = true;

            _panelBgStyle = new GUIStyle(GUI.skin.box);
            _panelBgStyle.normal.background = MakeTex(2, 2, new Color(0.08f, 0.08f, 0.12f, 0.95f));

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _titleStyle.normal.textColor = new Color(0.92f, 0.92f, 0.92f);

            _itemStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft
            };

            _selectedItemStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };

            _footerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter
            };
            _footerStyle.normal.textColor = new Color(0.55f, 0.55f, 0.55f);
        }

        private void DrawSeparator(float width)
        {
            Rect rect = GUILayoutUtility.GetRect(width, 1f);
            GUI.DrawTexture(rect, MakeTex(1, 1, new Color(0.4f, 0.4f, 0.4f, 0.6f)));
        }

        /// <summary>
        /// Boosts dark colors so they remain legible on a dark background.
        /// </summary>
        private static Color EnsureReadable(Color c)
        {
            float luminance = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
            if (luminance < 0.35f)
            {
                float boost = 0.35f / Mathf.Max(luminance, 0.01f);
                c.r = Mathf.Clamp01(c.r * boost);
                c.g = Mathf.Clamp01(c.g * boost);
                c.b = Mathf.Clamp01(c.b * boost);
            }
            return c;
        }

        private static Dictionary<Color, Texture2D> _texCache = new Dictionary<Color, Texture2D>();
        private static Texture2D MakeTex(int width, int height, Color col)
        {
            if (_texCache.TryGetValue(col, out var cached) && cached != null)
                return cached;

            var pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;
            var tex = new Texture2D(width, height);
            tex.SetPixels(pix);
            tex.Apply();
            _texCache[col] = tex;
            return tex;
        }
    }
}
