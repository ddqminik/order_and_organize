using OrderAndOrganize.Models;
using OrderAndOrganize.Services;
using Xunit;

namespace OrderAndOrganize.Tests
{
    public class ModelTests
    {
        [Theory]
        [InlineData(18, 19, 0, 37)]
        [InlineData(0, 0, 0, 0)]
        [InlineData(50, 50, 50, 150)]
        [InlineData(1, 2, 3, 6)]
        public void CombinedStock_SumsAllThreeValues(int shelves, int storage, int movement, int expected)
        {
            var snapshot = new ProductStockSnapshot
            {
                OnShelves = shelves,
                InStorage = storage,
                InMovement = movement
            };
            Assert.Equal(expected, snapshot.CombinedStock);
        }

        [Theory]
        [InlineData(18, 19, 0, 0, 37)]
        [InlineData(18, 19, 0, 30, 67)]
        [InlineData(0, 0, 0, 50, 50)]
        public void EffectiveCombinedStock_IncludesPending(
            int shelves, int storage, int movement, int pending, int expected)
        {
            var snapshot = new ProductStockSnapshot
            {
                OnShelves = shelves,
                InStorage = storage,
                InMovement = movement,
                PendingUnreflectedUnits = pending
            };
            Assert.Equal(expected, snapshot.EffectiveCombinedStock);
        }

        [Fact]
        public void Shortage_IsThresholdMinusCombined()
        {
            var candidate = new ProductOrderCandidate
            {
                CombinedStock = 17,
                Threshold = 40
            };
            Assert.Equal(23, candidate.Shortage);
        }

        [Fact]
        public void PurchaseResult_SucceededFactory()
        {
            var result = PurchaseResult.Succeeded(1, "TestProduct", 50f, 200f, 150f);
            Assert.True(result.Success);
            Assert.Equal(1, result.ProductId);
            Assert.Equal("TestProduct", result.ProductName);
            Assert.Equal(50f, result.BoxPrice);
            Assert.Equal(200f, result.MoneyBefore);
            Assert.Equal(150f, result.MoneyAfter);
            Assert.Null(result.FailureReason);
        }

        [Fact]
        public void PurchaseResult_FailedFactory()
        {
            var result = PurchaseResult.Failed(2, "Other", 75f, "Not enough money");
            Assert.False(result.Success);
            Assert.Equal(2, result.ProductId);
            Assert.Equal("Not enough money", result.FailureReason);
        }

        [Fact]
        public void AutomationCycleResult_HasPurchases()
        {
            var result = new AutomationCycleResult { ProductsPurchased = 1 };
            Assert.True(result.HasPurchases);

            result.ProductsPurchased = 0;
            Assert.False(result.HasPurchases);
        }

        [Fact]
        public void AutomationCycleResult_HasSkips()
        {
            var result = new AutomationCycleResult();
            Assert.False(result.HasSkips);

            result.ProductsSkippedInsufficientFunds = 1;
            Assert.True(result.HasSkips);
        }
    }

    public class CashReserveTests
    {
        [Theory]
        [InlineData(100f, 0f, 80f, true)]    // 100-0=100 >= 80
        [InlineData(100f, 30f, 80f, false)]   // 100-30=70 < 80
        [InlineData(50f, 0f, 50f, true)]      // 50-0=50 >= 50
        [InlineData(49f, 0f, 50f, false)]     // 49-0=49 < 50
        [InlineData(220f, 50f, 168.30f, true)] // 220-50=170 >= 168.30
        [InlineData(200f, 50f, 168.30f, false)] // 200-50=150 < 168.30
        public void CashReserve_AffordabilityCheck(float money, float reserve, float price, bool canAfford)
        {
            float spendable = money - reserve;
            if (spendable < 0) spendable = 0;
            bool result = price <= spendable;
            Assert.Equal(canAfford, result);
        }

        [Theory]
        [InlineData(100f, 0f, 80f, 20f)]     // 100-80=20 remaining
        [InlineData(50f, 0f, 50f, 0f)]        // 50-50=0 remaining
        public void MoneyAfterPurchase_NeverBelowReserve(
            float money, float reserve, float price, float expectedRemaining)
        {
            float spendable = money - reserve;
            Assert.True(price <= spendable, "Should be able to afford");
            float remaining = money - price;
            Assert.Equal(expectedRemaining, remaining);
            Assert.True(remaining >= reserve, "Remaining should be >= reserve");
        }
    }

    public class CandidateSortingTests
    {
        [Fact]
        public void MultipleCandidates_PurchaseOrder()
        {
            var snapshots = new[]
            {
                new ProductStockSnapshot
                {
                    ProductId = 3, ProductName = "C", OnShelves = 30, InStorage = 0, InMovement = 0,
                    UnitsPerBox = 10, BoxPrice = 80f, IsUnlocked = true, IsOrderable = true
                },
                new ProductStockSnapshot
                {
                    ProductId = 1, ProductName = "A", OnShelves = 2, InStorage = 0, InMovement = 0,
                    UnitsPerBox = 10, BoxPrice = 60f, IsUnlocked = true, IsOrderable = true
                },
                new ProductStockSnapshot
                {
                    ProductId = 2, ProductName = "B", OnShelves = 10, InStorage = 0, InMovement = 0,
                    UnitsPerBox = 10, BoxPrice = 70f, IsUnlocked = true, IsOrderable = true
                }
            };

            var planner = new RestockPlanner();
            var candidates = planner.PlanManualRestock(snapshots, 40);

            Assert.Equal(3, candidates.Count);
            Assert.Equal(1, candidates[0].ProductId);  // A: stock 2
            Assert.Equal(2, candidates[1].ProductId);  // B: stock 10
            Assert.Equal(3, candidates[2].ProductId);  // C: stock 30
        }

        [Fact]
        public void MultipleCandidates_BudgetConstraint()
        {
            // A: price 60, B: price 70, C: price 80
            // Money: 130 => buy A (70 left), buy B (0 left), skip C
            float money = 130f;
            float reserve = 0f;
            float spent = 0f;
            int purchased = 0;

            float[] prices = { 60f, 70f, 80f };

            foreach (float price in prices)
            {
                float spendable = money - spent - reserve;
                if (price <= spendable)
                {
                    spent += price;
                    purchased++;
                }
            }

            Assert.Equal(2, purchased);
            Assert.Equal(130f, spent);
            Assert.Equal(0f, money - spent);
        }
    }
}
