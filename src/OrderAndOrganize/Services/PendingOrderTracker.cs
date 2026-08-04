using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using OrderAndOrganize.Models;

namespace OrderAndOrganize.Services
{
    public class PendingOrderTracker
    {
        private readonly ManualLogSource _log;
        private readonly Dictionary<int, PendingAutomatedOrder> _pendingOrders =
            new Dictionary<int, PendingAutomatedOrder>();

        public PendingOrderTracker(ManualLogSource log)
        {
            _log = log;
        }

        public void RecordOrder(ProductStockSnapshot snapshot, float boxPrice)
        {
            var order = new PendingAutomatedOrder
            {
                ProductId = snapshot.ProductId,
                ProductName = snapshot.ProductName,
                OrderedUnits = snapshot.UnitsPerBox,
                UnitsPerBox = snapshot.UnitsPerBox,
                PurchaseTimestamp = DateTime.UtcNow,
                CombinedStockBeforePurchase = snapshot.CombinedStock,
                InMovementBeforePurchase = snapshot.InMovement,
                ExpectedCombinedStock = snapshot.CombinedStock + snapshot.UnitsPerBox,
                BoxPrice = boxPrice
            };

            _pendingOrders[snapshot.ProductId] = order;
            _log?.LogInfo(
                $"Pending order recorded: {snapshot.ProductName} (ID={snapshot.ProductId}), " +
                $"Units={order.OrderedUnits}, Expected total={order.ExpectedCombinedStock}");
        }

        public bool HasPendingOrder(int productId)
        {
            return _pendingOrders.ContainsKey(productId);
        }

        public PendingAutomatedOrder GetPendingOrder(int productId)
        {
            _pendingOrders.TryGetValue(productId, out var order);
            return order;
        }

        /// <summary>
        /// Reconciles pending orders against current game state.
        /// Resolves orders when InMovement reflects the purchase, or handles timeouts.
        /// </summary>
        public void Reconcile(
            IEnumerable<ProductStockSnapshot> currentSnapshots,
            double timeoutSeconds)
        {
            var toRemove = new List<int>();
            var snapshotDict = new Dictionary<int, ProductStockSnapshot>();

            foreach (var snap in currentSnapshots)
                snapshotDict[snap.ProductId] = snap;

            foreach (var kvp in _pendingOrders)
            {
                int productId = kvp.Key;
                var pending = kvp.Value;

                if (!snapshotDict.TryGetValue(productId, out var current))
                {
                    _log?.LogDebug($"Pending order for {pending.ProductName}: product no longer in scan results.");
                    continue;
                }

                bool reflected = current.InMovement >= pending.InMovementBeforePurchase + pending.OrderedUnits
                              || current.CombinedStock >= pending.ExpectedCombinedStock;

                if (reflected)
                {
                    _log?.LogInfo(
                        $"Pending order resolved: {pending.ProductName} (ID={productId}). " +
                        $"InMovement: {pending.InMovementBeforePurchase} -> {current.InMovement}, " +
                        $"CombinedStock: {pending.CombinedStockBeforePurchase} -> {current.CombinedStock}");
                    toRemove.Add(productId);
                    continue;
                }

                if (pending.IsTimedOut(timeoutSeconds))
                {
                    bool stockIncreased = current.CombinedStock > pending.CombinedStockBeforePurchase;

                    _log?.LogWarning(
                        $"Pending order timed out: {pending.ProductName} (ID={productId}), " +
                        $"age={((DateTime.UtcNow - pending.PurchaseTimestamp).TotalSeconds):F0}s. " +
                        $"Stock before={pending.CombinedStockBeforePurchase}, now={current.CombinedStock}. " +
                        $"InMovement before={pending.InMovementBeforePurchase}, now={current.InMovement}. " +
                        $"Removing only if no evidence of original order.");

                    if (!stockIncreased && current.InMovement <= pending.InMovementBeforePurchase)
                    {
                        _log?.LogInfo($"Timeout: no evidence of delivery for {pending.ProductName}. Allowing re-order.");
                        toRemove.Add(productId);
                    }
                    else
                    {
                        _log?.LogInfo($"Timeout: stock or InMovement changed for {pending.ProductName}. Keeping pending record.");
                    }
                }
            }

            foreach (int id in toRemove)
                _pendingOrders.Remove(id);
        }

        public void ClearAll()
        {
            int count = _pendingOrders.Count;
            _pendingOrders.Clear();
            if (count > 0)
                _log?.LogInfo($"Cleared {count} pending order records.");
        }

        public int Count => _pendingOrders.Count;
    }
}
