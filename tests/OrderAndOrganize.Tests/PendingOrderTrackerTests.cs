using System;
using System.Collections.Generic;
using OrderAndOrganize.Models;
using OrderAndOrganize.Services;
using Xunit;

namespace OrderAndOrganize.Tests
{
    public class PendingOrderTrackerTests
    {
        private readonly PendingOrderTracker _tracker = new PendingOrderTracker(null);

        private static ProductStockSnapshot MakeSnapshot(
            int id, int shelves = 0, int storage = 0, int movement = 0, int unitsPerBox = 30)
        {
            return new ProductStockSnapshot
            {
                ProductId = id,
                ProductName = $"Product_{id}",
                OnShelves = shelves,
                InStorage = storage,
                InMovement = movement,
                UnitsPerBox = unitsPerBox,
                BoxPrice = 100f,
                IsUnlocked = true,
                IsOrderable = true
            };
        }

        [Fact]
        public void RecordOrder_CreatesEntry()
        {
            var snapshot = MakeSnapshot(42, shelves: 18, storage: 19, movement: 0);
            _tracker.RecordOrder(snapshot, 168.30f);

            Assert.True(_tracker.HasPendingOrder(42));
            var pending = _tracker.GetPendingOrder(42);
            Assert.NotNull(pending);
            Assert.Equal(42, pending.ProductId);
            Assert.Equal(30, pending.OrderedUnits);
            Assert.Equal(37, pending.CombinedStockBeforePurchase);
            Assert.Equal(0, pending.InMovementBeforePurchase);
            Assert.Equal(67, pending.ExpectedCombinedStock);
            Assert.Equal(168.30f, pending.BoxPrice);
        }

        [Fact]
        public void HasPendingOrder_ReturnsFalseWhenNone()
        {
            Assert.False(_tracker.HasPendingOrder(99));
        }

        [Fact]
        public void Reconcile_ResolvesWhenInMovementReflects()
        {
            var snapshot = MakeSnapshot(42, shelves: 18, storage: 19, movement: 0);
            _tracker.RecordOrder(snapshot, 100f);

            Assert.True(_tracker.HasPendingOrder(42));

            var updatedSnapshots = new List<ProductStockSnapshot>
            {
                new ProductStockSnapshot
                {
                    ProductId = 42,
                    ProductName = "Product_42",
                    OnShelves = 18,
                    InStorage = 19,
                    InMovement = 30,
                    UnitsPerBox = 30,
                    BoxPrice = 100f,
                    IsUnlocked = true,
                    IsOrderable = true
                }
            };

            _tracker.Reconcile(updatedSnapshots, 120);

            Assert.False(_tracker.HasPendingOrder(42));
        }

        [Fact]
        public void Reconcile_KeepsPendingWhenNotReflected()
        {
            var snapshot = MakeSnapshot(42, shelves: 18, storage: 19, movement: 0);
            _tracker.RecordOrder(snapshot, 100f);

            var unchangedSnapshots = new List<ProductStockSnapshot>
            {
                MakeSnapshot(42, shelves: 18, storage: 19, movement: 0)
            };

            _tracker.Reconcile(unchangedSnapshots, 120);

            Assert.True(_tracker.HasPendingOrder(42));
        }

        [Fact]
        public void ClearAll_RemovesEverything()
        {
            _tracker.RecordOrder(MakeSnapshot(1), 50f);
            _tracker.RecordOrder(MakeSnapshot(2), 60f);
            Assert.Equal(2, _tracker.Count);

            _tracker.ClearAll();
            Assert.Equal(0, _tracker.Count);
            Assert.False(_tracker.HasPendingOrder(1));
            Assert.False(_tracker.HasPendingOrder(2));
        }

        [Fact]
        public void PendingOrder_IsTimedOut()
        {
            var order = new PendingAutomatedOrder
            {
                ProductId = 1,
                PurchaseTimestamp = DateTime.UtcNow.AddSeconds(-200)
            };

            Assert.True(order.IsTimedOut(120));
            Assert.False(order.IsTimedOut(300));
        }
    }
}
