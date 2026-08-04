using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Logging;
using OrderAndOrganize.Game;
using OrderAndOrganize.Models;
using UnityEngine;

namespace OrderAndOrganize.Services
{
    public class PurchaseService
    {
        private readonly ManualLogSource _log;
        private readonly GamePurchaseAdapter _purchaseAdapter;
        private readonly GameMoneyAdapter _money;
        private readonly PendingOrderTracker _pendingTracker;
        private readonly bool _verbose;

        public PurchaseService(
            ManualLogSource log,
            GamePurchaseAdapter purchaseAdapter,
            GameMoneyAdapter money,
            PendingOrderTracker pendingTracker,
            bool verbose)
        {
            _log = log;
            _purchaseAdapter = purchaseAdapter;
            _money = money;
            _pendingTracker = pendingTracker;
            _verbose = verbose;
        }

        /// <summary>
        /// Purchases candidates one at a time via the native BuyCargo flow.
        /// Must run as a coroutine. Populates the result parameter.
        /// </summary>
        public IEnumerator ExecutePurchases(
            ManagerBlackboard blackboard,
            IReadOnlyList<ProductOrderCandidate> candidates,
            IReadOnlyList<ProductStockSnapshot> snapshots,
            float cashReserve,
            AutomationCycleResult result)
        {
            var snapshotDict = new Dictionary<int, ProductStockSnapshot>();
            foreach (var s in snapshots)
                snapshotDict[s.ProductId] = s;

            result.MoneyBefore = _money.GetCurrentMoney();

            foreach (var candidate in candidates)
            {
                float currentMoney = _money.GetCurrentMoney();
                float spendable = currentMoney - cashReserve;

                if (candidate.BoxPrice > spendable)
                {
                    result.ProductsSkippedInsufficientFunds++;
                    if (_verbose)
                    {
                        _log.LogDebug(
                            $"Skipping {candidate.ProductName}: price={candidate.BoxPrice:F2}, " +
                            $"spendable={spendable:F2} (money={currentMoney:F2}, reserve={cashReserve:F2})");
                    }
                    continue;
                }

                if (!_purchaseAdapter.IsShoppingListEmpty(blackboard))
                {
                    result.ProductsSkippedUnavailable++;
                    _log.LogInfo("Shopping list has manual entries; halting automated purchases.");
                    break;
                }

                PurchaseResult purchaseResult = null;
                yield return CoroutineRunner.Instance.StartTrackedCoroutine(
                    _purchaseAdapter.PurchaseSingleProduct(
                        blackboard,
                        candidate.ProductId,
                        candidate.ProductName,
                        candidate.BoxPrice,
                        cashReserve,
                        r => purchaseResult = r));

                if (purchaseResult == null)
                {
                    result.Errors.Add($"No result returned for {candidate.ProductName}");
                    continue;
                }

                if (purchaseResult.Success)
                {
                    result.ProductsPurchased++;
                    result.TotalSpent += candidate.BoxPrice;

                    if (snapshotDict.TryGetValue(candidate.ProductId, out var snapshot))
                    {
                        _pendingTracker.RecordOrder(snapshot, candidate.BoxPrice);
                    }

                    if (_verbose)
                    {
                        _log.LogDebug(
                            $"Purchased: {candidate.ProductName} (ID={candidate.ProductId}), " +
                            $"Price={candidate.BoxPrice:F2}, " +
                            $"MoneyBefore={purchaseResult.MoneyBefore:F2}, " +
                            $"MoneyAfter={purchaseResult.MoneyAfter:F2}");
                    }
                }
                else
                {
                    _log.LogDebug($"Purchase failed for {candidate.ProductName}: {purchaseResult.FailureReason}");
                    if (purchaseResult.FailureReason?.Contains("Insufficient funds") == true ||
                        purchaseResult.FailureReason?.Contains("Funds changed") == true)
                    {
                        result.ProductsSkippedInsufficientFunds++;
                    }
                    else
                    {
                        result.ProductsSkippedUnavailable++;
                    }
                }
            }

            result.MoneyAfter = _money.GetCurrentMoney();
        }
    }
}
