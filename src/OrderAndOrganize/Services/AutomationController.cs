using System;
using System.Collections;
using BepInEx.Logging;
using OrderAndOrganize.Configuration;
using OrderAndOrganize.Game;
using OrderAndOrganize.Models;
using UnityEngine;

namespace OrderAndOrganize.Services
{
    public class AutomationController
    {
        private readonly ManualLogSource _log;
        private readonly ModConfiguration _config;
        private readonly GameAuthorityAdapter _authority;
        private readonly GameMoneyAdapter _money;
        private readonly GamePurchaseAdapter _purchase;
        private readonly GameProductCatalogAdapter _catalog;
        private readonly GameInventoryAdapter _inventory;
        private readonly GameShoppingListAdapter _shoppingList;
        private readonly GameNotificationAdapter _notification;
        private readonly PendingOrderTracker _pendingTracker;

        private bool _enabled;
        private bool _isRunningCycle;
        private Coroutine _automationCoroutine;
        private bool _notifiedNotAuthority;

        public bool IsEnabled => _enabled;

        public AutomationController(
            ManualLogSource log,
            ModConfiguration config,
            GameAuthorityAdapter authority,
            GameMoneyAdapter money,
            GamePurchaseAdapter purchase,
            GameProductCatalogAdapter catalog,
            GameInventoryAdapter inventory,
            GameShoppingListAdapter shoppingList,
            GameNotificationAdapter notification,
            PendingOrderTracker pendingTracker)
        {
            _log = log;
            _config = config;
            _authority = authority;
            _money = money;
            _purchase = purchase;
            _catalog = catalog;
            _inventory = inventory;
            _shoppingList = shoppingList;
            _notification = notification;
            _pendingTracker = pendingTracker;
        }

        public void Toggle(MonoBehaviour host)
        {
            _enabled = !_enabled;
            _log.LogInfo($"Automation toggled: {(_enabled ? "ENABLED" : "DISABLED")}");

            var notifyService = new NotificationService(_notification, _config.ShowNotifications.Value);
            if (_enabled)
            {
                notifyService.NotifyAutomationEnabled();
                StartAutomation(host);
            }
            else
            {
                notifyService.NotifyAutomationDisabled();
                StopAutomation(host);
            }
        }

        public void SetEnabled(bool enabled, MonoBehaviour host)
        {
            if (_enabled == enabled) return;
            _enabled = enabled;
            _log.LogInfo($"Automation set: {(_enabled ? "ENABLED" : "DISABLED")}");

            if (_enabled)
                StartAutomation(host);
            else
                StopAutomation(host);
        }

        private void StartAutomation(MonoBehaviour host)
        {
            if (_automationCoroutine != null) return;
            _automationCoroutine = host.StartCoroutine(AutomationLoop());
            _log.LogInfo("Automation coroutine started.");
        }

        public void StopAutomation(MonoBehaviour host)
        {
            if (_automationCoroutine != null)
            {
                host.StopCoroutine(_automationCoroutine);
                _automationCoroutine = null;
                _log.LogInfo("Automation coroutine stopped.");
            }
            _isRunningCycle = false;
        }

        public void OnSceneUnload()
        {
            _pendingTracker.ClearAll();
            _isRunningCycle = false;
            _notifiedNotAuthority = false;
            _log.LogInfo("Scene unloaded: cleared automation state.");
        }

        private IEnumerator AutomationLoop()
        {
            while (_enabled)
            {
                yield return new WaitForSeconds(_config.ScanIntervalSeconds.Value);

                if (!_enabled) break;

                if (!_authority.IsStoreLoaded())
                {
                    _log.LogDebug("Automation: no store loaded, skipping cycle.");
                    continue;
                }

                if (!_authority.IsLocalAuthority())
                {
                    if (!_notifiedNotAuthority)
                    {
                        _log.LogInfo("Automation: not the host/server. Automatic purchases disabled.");
                        var notifyService = new NotificationService(_notification, _config.ShowNotifications.Value);
                        notifyService.NotifyNotAuthority();
                        _notifiedNotAuthority = true;
                    }
                    continue;
                }

                if (_isRunningCycle)
                {
                    _log.LogDebug("Automation: previous cycle still running, skipping.");
                    continue;
                }

                yield return RunCycle();
            }

            _automationCoroutine = null;
        }

        private IEnumerator RunCycle()
        {
            _isRunningCycle = true;

            var blackboard = _catalog.GetManagerBlackboard();
            if (blackboard == null)
            {
                _log.LogDebug("Automation: ManagerBlackboard not available.");
                _isRunningCycle = false;
                yield break;
            }

            if (!_purchase.IsShoppingListEmpty(blackboard))
            {
                var notifyService = new NotificationService(_notification, _config.ShowNotifications.Value);
                notifyService.NotifyManualListBlocking();
                _isRunningCycle = false;
                yield break;
            }

            int threshold = _config.ThresholdUnits.Value;
            bool verbose = _config.VerboseLogging.Value;

            System.Collections.Generic.List<ProductStockSnapshot> snapshots = null;
            System.Collections.Generic.List<ProductOrderCandidate> candidates = null;
            AutomationCycleResult result = null;

            try
            {
                var scanner = new InventoryScanner(
                    _log, _inventory, _catalog, _shoppingList, _pendingTracker, verbose);
                snapshots = scanner.ScanAll();

                _pendingTracker.Reconcile(snapshots, _config.PendingOrderTimeoutSeconds.Value);

                var planner = new RestockPlanner();
                candidates = planner.PlanAutoRestock(snapshots, threshold, _pendingTracker);

                result = new AutomationCycleResult
                {
                    ProductsScanned = snapshots.Count,
                    ProductsBelowThreshold = candidates.Count
                };
            }
            catch (Exception ex)
            {
                _log.LogError($"Automation scan/plan error: {ex}");
                _isRunningCycle = false;
                yield break;
            }

            if (candidates == null || candidates.Count == 0)
            {
                _log.LogDebug($"Automation: no products below threshold ({threshold}).");
                _isRunningCycle = false;
                yield break;
            }

            float cashReserve = _config.CashReserve.Value;

            var purchaseService = new PurchaseService(
                _log, _purchase, _money, _pendingTracker, verbose);
            yield return CoroutineRunner.Instance.StartTrackedCoroutine(
                purchaseService.ExecutePurchases(blackboard, candidates, snapshots, cashReserve, result));

            _log.LogInfo(
                $"Automation cycle complete: scanned={result.ProductsScanned}, " +
                $"below threshold={result.ProductsBelowThreshold}, purchased={result.ProductsPurchased}, " +
                $"skipped(funds)={result.ProductsSkippedInsufficientFunds}, " +
                $"skipped(pending)={result.ProductsSkippedPending}, " +
                $"spent=${result.TotalSpent:F2}, " +
                $"money={result.MoneyBefore:F2}->{result.MoneyAfter:F2}");

            var notifySvc = new NotificationService(_notification, _config.ShowNotifications.Value);
            notifySvc.NotifyAutomationResult(result);

            _isRunningCycle = false;
        }
    }
}
