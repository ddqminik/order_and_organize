using System.Collections.Generic;
using System.Linq;
using OrderAndOrganize.Models;
using OrderAndOrganize.Services;
using Xunit;

namespace OrderAndOrganize.Tests
{
    public class RestockPlannerTests
    {
        private readonly RestockPlanner _planner = new RestockPlanner();

        private static ProductStockSnapshot MakeSnapshot(
            int id, int onShelves, int inStorage, int inMovement,
            int unitsPerBox = 30, float boxPrice = 100f,
            bool unlocked = true, bool orderable = true, bool onList = false,
            int pendingUnits = 0)
        {
            return new ProductStockSnapshot
            {
                ProductId = id,
                ProductName = $"Product_{id}",
                OnShelves = onShelves,
                InStorage = inStorage,
                InMovement = inMovement,
                PendingUnreflectedUnits = pendingUnits,
                UnitsPerBox = unitsPerBox,
                BoxPrice = boxPrice,
                IsUnlocked = unlocked,
                IsOrderable = orderable,
                IsOnShoppingList = onList
            };
        }

        [Theory]
        [InlineData(18, 19, 0, 37, true)]   // 37 < 40 -> include
        [InlineData(18, 19, 3, 40, false)]   // 40 < 40 -> exclude (strict)
        [InlineData(0, 0, 39, 39, true)]     // 39 < 40 -> include
        [InlineData(0, 0, 40, 40, false)]    // 40 < 40 -> exclude
        [InlineData(50, 0, 0, 50, false)]    // 50 < 40 -> exclude
        [InlineData(0, 0, 0, 0, true)]       // 0 < 40 -> include
        public void CombinedStock_ThresholdBoundary(
            int shelves, int storage, int movement, int expectedCombined, bool shouldQualify)
        {
            var snapshot = MakeSnapshot(1, shelves, storage, movement);
            Assert.Equal(expectedCombined, snapshot.CombinedStock);

            var candidates = _planner.PlanManualRestock(new[] { snapshot }, 40);
            Assert.Equal(shouldQualify, candidates.Count > 0);
        }

        [Fact]
        public void ManualRestock_ExcludesLockedProducts()
        {
            var snapshot = MakeSnapshot(1, 0, 0, 0, unlocked: false);
            var result = _planner.PlanManualRestock(new[] { snapshot }, 40);
            Assert.Empty(result);
        }

        [Fact]
        public void ManualRestock_ExcludesUnorderableProducts()
        {
            var snapshot = MakeSnapshot(1, 0, 0, 0, orderable: false);
            var result = _planner.PlanManualRestock(new[] { snapshot }, 40);
            Assert.Empty(result);
        }

        [Fact]
        public void ManualRestock_ExcludesProductsAlreadyOnShoppingList()
        {
            var snapshot = MakeSnapshot(1, 10, 0, 0, onList: true);
            var result = _planner.PlanManualRestock(new[] { snapshot }, 40);
            Assert.Empty(result);
        }

        [Fact]
        public void ManualRestock_SortsByLowestStockFirst()
        {
            var snapshots = new[]
            {
                MakeSnapshot(3, 31, 0, 0),  // combined = 31
                MakeSnapshot(1, 2, 0, 0),   // combined = 2
                MakeSnapshot(2, 18, 0, 0),  // combined = 18
            };

            var result = _planner.PlanManualRestock(snapshots, 40);

            Assert.Equal(3, result.Count);
            Assert.Equal(1, result[0].ProductId);  // stock 2
            Assert.Equal(2, result[1].ProductId);  // stock 18
            Assert.Equal(3, result[2].ProductId);  // stock 31
        }

        [Fact]
        public void ManualRestock_TieBreaksByShortageDescendingThenProductIdAscending()
        {
            var snapshots = new[]
            {
                MakeSnapshot(5, 10, 0, 0),  // combined=10, shortage=30
                MakeSnapshot(3, 10, 0, 0),  // combined=10, shortage=30
                MakeSnapshot(1, 10, 0, 0),  // combined=10, shortage=30
            };

            var result = _planner.PlanManualRestock(snapshots, 40);

            Assert.Equal(3, result.Count);
            Assert.Equal(1, result[0].ProductId);
            Assert.Equal(3, result[1].ProductId);
            Assert.Equal(5, result[2].ProductId);
        }

        [Fact]
        public void ManualRestock_EmptySnapshotCollection()
        {
            var result = _planner.PlanManualRestock(new List<ProductStockSnapshot>(), 40);
            Assert.Empty(result);
        }

        [Fact]
        public void ManualRestock_ThresholdZero_NothingQualifies()
        {
            var snapshot = MakeSnapshot(1, 0, 0, 0);
            var result = _planner.PlanManualRestock(new[] { snapshot }, 0);
            Assert.Empty(result);
        }

        [Fact]
        public void Shortage_CalculatedCorrectly()
        {
            var snapshot = MakeSnapshot(1, 10, 5, 2);
            var candidates = _planner.PlanManualRestock(new[] { snapshot }, 40);

            Assert.Single(candidates);
            Assert.Equal(40 - 17, candidates[0].Shortage);  // 40 - (10+5+2) = 23
        }

        [Fact]
        public void EffectiveCombinedStock_IncludesPendingUnits()
        {
            var snapshot = MakeSnapshot(1, 18, 19, 0, pendingUnits: 30);
            Assert.Equal(37, snapshot.CombinedStock);
            Assert.Equal(67, snapshot.EffectiveCombinedStock);
        }

        [Fact]
        public void AutoRestock_UsesEffectiveCombinedStock()
        {
            var tracker = new PendingOrderTracker(null);
            var snapshot = MakeSnapshot(1, 18, 19, 0, pendingUnits: 30);
            // effective = 67, threshold = 40 -> should NOT qualify
            var result = _planner.PlanAutoRestock(new[] { snapshot }, 40, tracker);
            Assert.Empty(result);
        }

        [Fact]
        public void AutoRestock_ExcludesProductsWithPendingOrders()
        {
            var tracker = new PendingOrderTracker(null);
            var snapshot = MakeSnapshot(1, 10, 0, 0);

            tracker.RecordOrder(snapshot, 100f);
            var result = _planner.PlanAutoRestock(new[] { snapshot }, 40, tracker);
            Assert.Empty(result);
        }
    }
}
