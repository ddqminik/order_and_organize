using System.Collections.Generic;

namespace OrderAndOrganize.Models
{
    public class AutomationCycleResult
    {
        public int ProductsScanned { get; set; }
        public int ProductsBelowThreshold { get; set; }
        public int ProductsPurchased { get; set; }
        public int ProductsSkippedPending { get; set; }
        public int ProductsSkippedShoppingList { get; set; }
        public int ProductsSkippedInsufficientFunds { get; set; }
        public int ProductsSkippedUnavailable { get; set; }
        public float TotalSpent { get; set; }
        public float MoneyBefore { get; set; }
        public float MoneyAfter { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public bool HasPurchases => ProductsPurchased > 0;
        public bool HasSkips => ProductsSkippedInsufficientFunds > 0 ||
                                ProductsSkippedPending > 0 ||
                                ProductsSkippedShoppingList > 0;
    }
}
