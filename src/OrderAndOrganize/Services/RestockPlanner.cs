using System.Collections.Generic;
using System.Linq;
using OrderAndOrganize.Models;

namespace OrderAndOrganize.Services
{
    public class RestockPlanner
    {
        /// <summary>
        /// Selects products for manual mode (add to shopping list).
        /// Uses CombinedStock (not effective) and excludes products already on the shopping list.
        /// </summary>
        public List<ProductOrderCandidate> PlanManualRestock(
            IEnumerable<ProductStockSnapshot> snapshots, int threshold)
        {
            return snapshots
                .Where(s => s.IsUnlocked
                         && s.IsOrderable
                         && s.CombinedStock < threshold
                         && !s.IsOnShoppingList)
                .Select(s => ToCandidate(s, threshold))
                .OrderBy(c => c.CombinedStock)
                .ThenByDescending(c => c.Shortage)
                .ThenBy(c => c.ProductId)
                .ToList();
        }

        /// <summary>
        /// Selects products for automatic mode.
        /// Uses EffectiveCombinedStock and excludes products with unresolved pending orders.
        /// </summary>
        public List<ProductOrderCandidate> PlanAutoRestock(
            IEnumerable<ProductStockSnapshot> snapshots,
            int threshold,
            PendingOrderTracker pendingTracker)
        {
            return snapshots
                .Where(s => s.IsUnlocked
                         && s.IsOrderable
                         && s.EffectiveCombinedStock < threshold
                         && !pendingTracker.HasPendingOrder(s.ProductId))
                .Select(s => ToCandidate(s, threshold))
                .OrderBy(c => c.CombinedStock)
                .ThenByDescending(c => c.Shortage)
                .ThenBy(c => c.ProductId)
                .ToList();
        }

        private static ProductOrderCandidate ToCandidate(ProductStockSnapshot snapshot, int threshold)
        {
            return new ProductOrderCandidate
            {
                ProductId = snapshot.ProductId,
                ProductName = snapshot.ProductName,
                CombinedStock = snapshot.CombinedStock,
                EffectiveCombinedStock = snapshot.EffectiveCombinedStock,
                Threshold = threshold,
                UnitsPerBox = snapshot.UnitsPerBox,
                BoxPrice = snapshot.BoxPrice
            };
        }
    }
}
