using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OrderAndOrganize.Configuration;
using OrderAndOrganize.Diagnostics;
using OrderAndOrganize.Game;
using OrderAndOrganize.Patches;
using OrderAndOrganize.Services;
using OrderAndOrganize.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OrderAndOrganize
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.ddqminik.supermarkettogether.orderandorganize";
        public const string PluginName = "Order & Organize";
        public const string PluginVersion = "0.5.0";

        internal static ManualLogSource Log;
        internal static Plugin Instance;

        private ModConfiguration _config;
        private Harmony _harmony;

        private GameInventoryAdapter _inventoryAdapter;
        private GameProductCatalogAdapter _catalogAdapter;
        private GameShoppingListAdapter _shoppingListAdapter;
        private GamePurchaseAdapter _purchaseAdapter;
        private GameMoneyAdapter _moneyAdapter;
        private GameNotificationAdapter _notificationAdapter;
        private GameAuthorityAdapter _authorityAdapter;
        private GameUiAdapter _uiAdapter;

        private PendingOrderTracker _pendingTracker;
        private AutomationController _automationController;
        private OrderButtonController _buttonController;
        private GameApiDiagnostics _diagnostics;
        private ConfigWindow _configWindow;

        // Category shelf components
        private CategoryMapper _categoryMapper;
        private CategoryShelfManager _categoryShelfManager;
        private CategoryPickerUI _categoryPickerUI;

        private bool _hasLoggedApiOnce;
        private float _storeLoadedTime;
        private const float DiagnosticsDelaySeconds = 5f;

        private bool _categoryMapperRebuilt;
        private float _lastStalePurgeTime;
        private float _sceneLoadedTime;
        private const float PurgeGracePeriodSeconds = 120f;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            Log.LogInfo($"{PluginName} v{PluginVersion} loading...");

            MigrateFromOldVersion();

            _config = new ModConfiguration();
            _config.Bind(Config);

            if (!_config.Enabled.Value)
            {
                Log.LogInfo("Plugin is disabled via configuration.");
                return;
            }

            InitializeAdapters();
            InitializeServices();
            InitializeCategoryShelves();

            CoroutineRunner.Initialize();

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll();

            SceneManager.sceneUnloaded += OnSceneUnloaded;

            Log.LogInfo($"{PluginName} v{PluginVersion} loaded successfully.");
        }

        private void InitializeAdapters()
        {
            _inventoryAdapter = new GameInventoryAdapter(Log);
            _catalogAdapter = new GameProductCatalogAdapter(Log);
            _shoppingListAdapter = new GameShoppingListAdapter(Log);
            _moneyAdapter = new GameMoneyAdapter(Log);
            _notificationAdapter = new GameNotificationAdapter(Log);
            _authorityAdapter = new GameAuthorityAdapter(Log);
            _uiAdapter = new GameUiAdapter(Log);
            _purchaseAdapter = new GamePurchaseAdapter(Log, _shoppingListAdapter, _moneyAdapter);
        }

        private void InitializeServices()
        {
            _pendingTracker = new PendingOrderTracker(Log);

            _automationController = new AutomationController(
                Log, _config, _authorityAdapter, _moneyAdapter, _purchaseAdapter,
                _catalogAdapter, _inventoryAdapter, _shoppingListAdapter,
                _notificationAdapter, _pendingTracker);

            _buttonController = new OrderButtonController(
                Log, _config, _catalogAdapter, _inventoryAdapter,
                _shoppingListAdapter, _notificationAdapter, _pendingTracker,
                () => _automationController.IsEnabled);

            _diagnostics = new GameApiDiagnostics(
                Log, _inventoryAdapter, _catalogAdapter, _shoppingListAdapter);

            _configWindow = new ConfigWindow(
                _config,
                () => _automationController.IsEnabled,
                () =>
                {
                    _automationController.Toggle(this);
                    _buttonController.UpdateAutoLabel();
                });
        }

        private void InitializeCategoryShelves()
        {
            _categoryMapper = new CategoryMapper(Log);

            string savePath = Path.Combine(
                Path.GetDirectoryName(Config.ConfigFilePath),
                "OrderAndOrganize_CategoryShelves.json");

            _categoryShelfManager = new CategoryShelfManager(Log, _categoryMapper, savePath);
            _categoryPickerUI = new CategoryPickerUI(Log, _categoryShelfManager, _categoryMapper);
            _categoryPickerUI.LabelsVisible = _config.CategoryLabelsVisible.Value;

            // Wire up the static references for Harmony patches
            CategoryStoragePatches.ShelfManager = _categoryShelfManager;
            CategoryStoragePatches.CategoryMapper = _categoryMapper;
            CategoryStoragePatches.Enabled = _config.CategoryShelvesEnabled.Value;

            // React to config changes
            _config.CategoryShelvesEnabled.SettingChanged += (_, __) =>
            {
                CategoryStoragePatches.Enabled = _config.CategoryShelvesEnabled.Value;
                Log.LogInfo($"Category shelves {(_config.CategoryShelvesEnabled.Value ? "enabled" : "disabled")}.");
            };

            _config.CategoryLabelsVisible.SettingChanged += (_, __) =>
            {
                _categoryPickerUI.LabelsVisible = _config.CategoryLabelsVisible.Value;
            };

            Log.LogInfo("Category storage shelves initialized.");
        }

        private void Update()
        {
            if (!_config.Enabled.Value) return;

            // Block all game input while the category picker is open
            if (_categoryPickerUI != null && _categoryPickerUI.IsPickerVisible)
            {
                _categoryPickerUI.UpdateInput();
                return;
            }

            if (Input.GetKeyDown(_config.ToggleHotkey.Value))
            {
                _automationController.Toggle(this);
                _buttonController.UpdateAutoLabel();
            }

            if (Input.GetKeyDown(_config.ConfigWindowHotkey.Value))
            {
                _configWindow?.Toggle();
            }

            // Category hotkey: open picker when looking at a storage shelf
            if (_config.CategoryShelvesEnabled.Value && Input.GetKeyDown(_config.CategoryHotkey.Value))
            {
                _categoryPickerUI?.OnHotkeyPressed();
            }

            // Label toggle hotkey
            if (_config.CategoryShelvesEnabled.Value && Input.GetKeyDown(_config.CategoryLabelsToggleHotkey.Value))
            {
                if (_categoryPickerUI != null)
                {
                    _categoryPickerUI.ToggleLabels();
                    _config.CategoryLabelsVisible.Value = _categoryPickerUI.LabelsVisible;
                }
            }

            // Rebuild category mapper once ProductListing is available
            if (!_categoryMapperRebuilt && ProductListing.Instance != null
                && ProductListing.Instance.tiers != null
                && ProductListing.Instance.tiers.Length > 0)
            {
                _categoryMapperRebuilt = true;
                _categoryMapper.Rebuild();
            }

            // Purge stale category assignments every 60s, but only when the game scene
            // is fully loaded and props have had time to spawn (grace period).
            if (_categoryShelfManager != null && _categoryShelfManager.AssignmentCount > 0
                && GameData.Instance != null
                && _sceneLoadedTime > 0f
                && Time.time - _sceneLoadedTime > PurgeGracePeriodSeconds
                && Time.time - _lastStalePurgeTime > 60f)
            {
                _lastStalePurgeTime = Time.time;
                try { _categoryShelfManager.PurgeStaleAssignments(); }
                catch (Exception ex) { Log.LogWarning($"Stale purge failed: {ex.Message}"); }
            }

            HandleDiagnostics();
        }

        private void HandleDiagnostics()
        {
            if (_hasLoggedApiOnce) return;
            if (!_authorityAdapter.IsStoreLoaded()) return;

            if (_storeLoadedTime <= 0f)
            {
                _storeLoadedTime = Time.time;
                _sceneLoadedTime = Time.time;
                return;
            }

            if (Time.time - _storeLoadedTime < DiagnosticsDelaySeconds)
                return;

            _hasLoggedApiOnce = true;
            try
            {
                _diagnostics.LogResolvedApi();
            }
            catch (Exception ex)
            {
                Log.LogWarning($"API diagnostics failed: {ex.Message}");
            }
        }

        private void OnGUI()
        {
            _configWindow?.Draw();

            if (_config.CategoryShelvesEnabled.Value)
                _categoryPickerUI?.Draw();
        }

        private void OnSceneUnloaded(Scene scene)
        {
            Log.LogInfo($"Scene unloaded: {scene.name}");
            _automationController.OnSceneUnload();
            _hasLoggedApiOnce = false;
            _storeLoadedTime = 0f;
            _sceneLoadedTime = 0f;
            _lastStalePurgeTime = 0f;
            _categoryMapperRebuilt = false;
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            _pendingTracker?.ClearAll();
            Log.LogInfo($"{PluginName} unloaded.");
        }

        private void MigrateFromOldVersion()
        {
            try
            {
                string configDir = Path.GetDirectoryName(Config.ConfigFilePath);
                if (string.IsNullOrEmpty(configDir)) return;

                string oldCfg = Path.Combine(configDir, "com.dominik.supermarkettogether.smartrestock.cfg");
                string newCfg = Config.ConfigFilePath;
                if (File.Exists(oldCfg) && !File.Exists(newCfg))
                {
                    File.Copy(oldCfg, newCfg);
                    Log.LogInfo($"Migrated config from {Path.GetFileName(oldCfg)}");
                }

                string oldJson = Path.Combine(configDir, "SmartRestock_CategoryShelves.json");
                string newJson = Path.Combine(configDir, "OrderAndOrganize_CategoryShelves.json");
                if (File.Exists(oldJson) && !File.Exists(newJson))
                {
                    File.Copy(oldJson, newJson);
                    Log.LogInfo($"Migrated category data from {Path.GetFileName(oldJson)}");
                }

                string oldBackup = oldJson + ".backup";
                string newBackup = newJson + ".backup";
                if (File.Exists(oldBackup) && !File.Exists(newBackup))
                {
                    File.Copy(oldBackup, newBackup);
                }
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Migration from old version failed: {ex.Message}");
            }
        }

        internal void OnOrderingUIInitialized()
        {
            _buttonController?.TryCreateButton();
        }

        internal void OnAutoStartup()
        {
            if (_config.AutoOrderAtStartup.Value)
            {
                _automationController.SetEnabled(true, this);
                Log.LogInfo("Automation auto-started per configuration.");
            }
        }
    }

    [HarmonyPatch]
    internal static class GamePatches
    {
        [HarmonyPatch(typeof(ProductListing), "OnStartClient")]
        [HarmonyPostfix]
        static void OnProductListingStartClient()
        {
            try
            {
                Plugin.Log?.LogInfo("ProductListing.OnStartClient fired - initializing Order & Organize UI.");
                Plugin.Instance?.OnOrderingUIInitialized();
                Plugin.Instance?.OnAutoStartup();
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"Error in OnStartClient patch: {ex}");
            }
        }

        [HarmonyPatch(typeof(ManagerBlackboard), "UpdateUnlockedFranchises")]
        [HarmonyPostfix]
        static void OnUpdateUnlockedFranchises()
        {
            try
            {
                Plugin.Instance?.OnOrderingUIInitialized();
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"Error in UpdateUnlockedFranchises patch: {ex}");
            }
        }

        [HarmonyPatch(typeof(LocalizationManager), "GetLocalizationString")]
        [HarmonyPrefix]
        static bool OnGetLocalizationString(ref string key, ref string __result)
        {
            if (key != null && key.Length > 0 && key[0] == '`')
            {
                __result = key.Substring(1);
                return false;
            }
            return true;
        }
    }
}
