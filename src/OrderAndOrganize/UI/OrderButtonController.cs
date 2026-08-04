using System;
using BepInEx.Logging;
using OrderAndOrganize.Configuration;
using OrderAndOrganize.Game;
using OrderAndOrganize.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OrderAndOrganize.UI
{
    public class OrderButtonController
    {
        private const string ButtonObjectName = "OAO_AddLowStockButton";
        private const string AutoLabelObjectName = "OAO_AutoLabel";

        private readonly ManualLogSource _log;
        private readonly ModConfiguration _config;
        private readonly GameProductCatalogAdapter _catalog;
        private readonly GameInventoryAdapter _inventory;
        private readonly GameShoppingListAdapter _shoppingList;
        private readonly GameNotificationAdapter _notification;
        private readonly PendingOrderTracker _pendingTracker;
        private readonly Func<bool> _isAutoEnabled;

        private GameObject _buttonObj;
        private GameObject _autoLabelObj;
        private bool _isRunning;
        private bool _hasLoggedHierarchy;

        public OrderButtonController(
            ManualLogSource log,
            ModConfiguration config,
            GameProductCatalogAdapter catalog,
            GameInventoryAdapter inventory,
            GameShoppingListAdapter shoppingList,
            GameNotificationAdapter notification,
            PendingOrderTracker pendingTracker,
            Func<bool> isAutoEnabled)
        {
            _log = log;
            _config = config;
            _catalog = catalog;
            _inventory = inventory;
            _shoppingList = shoppingList;
            _notification = notification;
            _pendingTracker = pendingTracker;
            _isAutoEnabled = isAutoEnabled;
        }

        public void TryCreateButton()
        {
            try
            {
                var blackboard = _catalog.GetManagerBlackboard();
                if (blackboard == null) return;

                if (!_hasLoggedHierarchy)
                {
                    _hasLoggedHierarchy = true;
                    LogUiHierarchy(blackboard);
                }

                Transform buttonParent = FindButtonParent(blackboard);
                if (buttonParent == null)
                {
                    _log.LogWarning("Could not find a suitable parent for button placement.");
                    return;
                }

                if (buttonParent.Find(ButtonObjectName) != null)
                {
                    _log.LogDebug("Button already exists; skipping creation.");
                    UpdateAutoLabel();
                    return;
                }

                CreateButtonByCloning(blackboard, buttonParent);
                if (_buttonObj == null)
                    CreateButtonFallback(buttonParent);

                CreateAutoLabel(buttonParent);
                _log.LogInfo($"Order & Organize button created. Parent: {buttonParent.name}");
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed to create button: {ex}");
            }
        }

        private Transform FindButtonParent(ManagerBlackboard blackboard)
        {
            // The shoppingListParent is the right panel with the shopping list.
            // Its grandparent should be the main blackboard panel.
            if (blackboard.shoppingListParent != null)
            {
                Transform shoppingParent = blackboard.shoppingListParent.transform.parent;
                if (shoppingParent != null)
                {
                    _log.LogDebug($"Using shopping list grandparent: {shoppingParent.name}");
                    return shoppingParent;
                }
            }

            // Fallback: tabsOBJ parent
            if (blackboard.tabsOBJ != null)
            {
                return blackboard.tabsOBJ.transform.parent ?? blackboard.tabsOBJ.transform;
            }

            return blackboard.transform;
        }

        /// <summary>
        /// Try to find and clone an existing button (like "Buy Empty Box") for native styling.
        /// </summary>
        private void CreateButtonByCloning(ManagerBlackboard blackboard, Transform parent)
        {
            // Search for a clickable button in the blackboard hierarchy to clone
            GameObject referenceButton = FindReferenceButton(blackboard.transform);
            if (referenceButton == null)
            {
                _log.LogDebug("No reference button found to clone; will use fallback.");
                return;
            }

            _log.LogDebug($"Cloning reference button: {referenceButton.name}");

            _buttonObj = UnityEngine.Object.Instantiate(referenceButton, parent);
            _buttonObj.name = ButtonObjectName;

            // Remove any PlayMakerFSM to prevent original behavior
            foreach (var fsm in _buttonObj.GetComponents<PlayMakerFSM>())
                UnityEngine.Object.Destroy(fsm);
            foreach (var fsm in _buttonObj.GetComponentsInChildren<PlayMakerFSM>())
                UnityEngine.Object.Destroy(fsm);

            // Clear existing button listeners and add ours
            var button = _buttonObj.GetComponent<Button>();
            if (button == null)
                button = _buttonObj.AddComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClicked);

            // Update the text
            SetButtonText(_buttonObj, _config.ButtonText.Value);

            // Position near the bottom of the shopping list area
            var rect = _buttonObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(1f, 0f);
                rect.anchorMax = new Vector2(1f, 0f);
                rect.pivot = new Vector2(1f, 0f);
                rect.anchoredPosition = new Vector2(-10f, 10f);
                rect.sizeDelta = new Vector2(180f, 40f);
            }

            _log.LogInfo("Button created by cloning native button.");
        }

        private GameObject FindReferenceButton(Transform root)
        {
            // Search for buttons with known names in the blackboard
            foreach (Transform child in root)
            {
                // Look for common button-like names
                string name = child.name.ToLower();
                if (child.GetComponent<Button>() != null &&
                    (name.Contains("buy") || name.Contains("button") || name.Contains("btn")))
                {
                    return child.gameObject;
                }

                // Recurse one level deeper
                foreach (Transform grandchild in child)
                {
                    string gName = grandchild.name.ToLower();
                    if (grandchild.GetComponent<Button>() != null &&
                        (gName.Contains("buy") || gName.Contains("button") || gName.Contains("btn")))
                    {
                        return grandchild.gameObject;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Fallback: create a button from scratch with high-visibility styling.
        /// </summary>
        private void CreateButtonFallback(Transform parent)
        {
            _buttonObj = new GameObject(ButtonObjectName);
            _buttonObj.transform.SetParent(parent, false);

            var rect = _buttonObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-10f, 10f);
            rect.sizeDelta = new Vector2(180f, 40f);

            var image = _buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.6f, 0.2f, 1f);

            var button = _buttonObj.AddComponent<Button>();
            button.onClick.AddListener(OnButtonClicked);

            // ColorBlock for hover/press feedback
            var colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.6f, 0.2f, 1f);
            colors.highlightedColor = new Color(0.3f, 0.7f, 0.3f, 1f);
            colors.pressedColor = new Color(0.1f, 0.4f, 0.1f, 1f);
            button.colors = colors;

            var textObj = new GameObject("ButtonText");
            textObj.transform.SetParent(_buttonObj.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // Try TextMeshProUGUI first (matches native game text), fall back to legacy Text
            try
            {
                var tmp = textObj.AddComponent<TextMeshProUGUI>();
                tmp.text = _config.ButtonText.Value;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
                tmp.fontSize = 16f;
                tmp.fontStyle = FontStyles.Bold;
            }
            catch
            {
                var text = textObj.AddComponent<Text>();
                text.text = _config.ButtonText.Value;
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                text.alignment = TextAnchor.MiddleCenter;
                text.color = Color.white;
                text.fontStyle = FontStyle.Bold;
                text.fontSize = 16;
            }

            _log.LogInfo("Button created using fallback method.");
        }

        private void CreateAutoLabel(Transform parent)
        {
            _autoLabelObj = new GameObject(AutoLabelObjectName);
            _autoLabelObj.transform.SetParent(parent, false);

            var rect = _autoLabelObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-10f, 52f);
            rect.sizeDelta = new Vector2(180f, 20f);

            try
            {
                var tmp = _autoLabelObj.AddComponent<TextMeshProUGUI>();
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontSize = 12f;
            }
            catch
            {
                var text = _autoLabelObj.AddComponent<Text>();
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                text.alignment = TextAnchor.MiddleCenter;
                text.fontSize = 12;
            }

            UpdateAutoLabel();
        }

        public void UpdateAutoLabel()
        {
            if (_autoLabelObj == null) return;

            bool autoOn = _isAutoEnabled?.Invoke() ?? false;
            string labelText = autoOn ? "Auto Order: ON" : "Auto Order: OFF";
            Color labelColor = autoOn ? Color.green : Color.gray;

            var tmp = _autoLabelObj.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = labelText;
                tmp.color = labelColor;
                return;
            }

            var text = _autoLabelObj.GetComponent<Text>();
            if (text != null)
            {
                text.text = labelText;
                text.color = labelColor;
            }
        }

        private void SetButtonText(GameObject buttonObj, string newText)
        {
            var tmp = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = newText;
                return;
            }

            var text = buttonObj.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = newText;
            }
        }

        private void OnButtonClicked()
        {
            if (_isRunning) return;
            _isRunning = true;

            try
            {
                SetButtonInteractable(false);
                ExecuteManualRestock();
            }
            finally
            {
                SetButtonInteractable(true);
                _isRunning = false;
            }
        }

        private void ExecuteManualRestock()
        {
            int threshold = _config.ThresholdUnits.Value;
            bool verbose = _config.VerboseLogging.Value;

            var scanner = new InventoryScanner(
                _log, _inventory, _catalog, _shoppingList, _pendingTracker, verbose);
            var snapshots = scanner.ScanAll();

            var planner = new RestockPlanner();
            var candidates = planner.PlanManualRestock(snapshots, threshold);

            var shoppingListService = new ShoppingListService(_log, _shoppingList, _catalog);
            var (added, skipped) = shoppingListService.AddCandidatesToShoppingList(candidates);

            _log.LogInfo($"Manual restock: {added} added, {skipped} skipped, threshold={threshold}");

            var notifyService = new NotificationService(_notification, _config.ShowNotifications.Value);
            notifyService.NotifyManualResult(added, skipped, threshold);
        }

        private void SetButtonInteractable(bool interactable)
        {
            if (_buttonObj == null) return;
            var button = _buttonObj.GetComponent<Button>();
            if (button != null)
                button.interactable = interactable;
        }

        /// <summary>
        /// Dumps the ManagerBlackboard's UI hierarchy to the log for debugging.
        /// </summary>
        private void LogUiHierarchy(ManagerBlackboard blackboard)
        {
            _log.LogInfo("=== UI Hierarchy Dump (ManagerBlackboard) ===");

            _log.LogInfo($"tabsOBJ: {blackboard.tabsOBJ?.name ?? "NULL"}");
            _log.LogInfo($"tabsOBJ.parent: {blackboard.tabsOBJ?.transform?.parent?.name ?? "NULL"}");
            _log.LogInfo($"shopItemsParent: {blackboard.shopItemsParent?.name ?? "NULL"}");
            _log.LogInfo($"shopItemsParent.parent: {blackboard.shopItemsParent?.transform?.parent?.name ?? "NULL"}");
            _log.LogInfo($"shoppingListParent: {blackboard.shoppingListParent?.name ?? "NULL"}");
            _log.LogInfo($"shoppingListParent.parent: {blackboard.shoppingListParent?.transform?.parent?.name ?? "NULL"}");
            _log.LogInfo($"totalChargeOBJ: {blackboard.totalChargeOBJ?.name ?? "NULL"}");
            _log.LogInfo($"totalChargeOBJ.parent: {blackboard.totalChargeOBJ?.transform?.parent?.name ?? "NULL"}");

            // Log the full tree from ManagerBlackboard's root (2 levels deep)
            Transform root = blackboard.transform;
            _log.LogInfo($"ManagerBlackboard root: {root.name} (children={root.childCount})");
            LogChildren(root, 1, 3);

            // Also log parent chain up from shoppingListParent
            if (blackboard.shoppingListParent != null)
            {
                _log.LogInfo("--- Shopping list parent chain ---");
                Transform t = blackboard.shoppingListParent.transform;
                int depth = 0;
                while (t != null && depth < 8)
                {
                    var r = t.GetComponent<RectTransform>();
                    string posInfo = r != null
                        ? $"pos={r.anchoredPosition}, size={r.sizeDelta}, anchors=({r.anchorMin},{r.anchorMax})"
                        : "no RectTransform";
                    _log.LogInfo($"  [depth {depth}] {t.name} ({posInfo})");
                    t = t.parent;
                    depth++;
                }
            }

            _log.LogInfo("=== End UI Hierarchy Dump ===");
        }

        private void LogChildren(Transform parent, int currentDepth, int maxDepth)
        {
            if (currentDepth > maxDepth) return;

            string indent = new string(' ', currentDepth * 2);
            foreach (Transform child in parent)
            {
                var r = child.GetComponent<RectTransform>();
                bool hasButton = child.GetComponent<Button>() != null;
                bool hasTMP = child.GetComponent<TextMeshProUGUI>() != null;
                string extras = "";
                if (hasButton) extras += " [Button]";
                if (hasTMP) extras += $" [TMP: {child.GetComponent<TextMeshProUGUI>()?.text}]";
                string posInfo = r != null ? $" pos={r.anchoredPosition}" : "";

                _log.LogDebug($"{indent}{child.name} (children={child.childCount}){posInfo}{extras}");
                LogChildren(child, currentDepth + 1, maxDepth);
            }
        }
    }
}
