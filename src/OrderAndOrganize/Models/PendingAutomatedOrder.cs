using System;

namespace OrderAndOrganize.Models
{
    public class PendingAutomatedOrder
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int OrderedUnits { get; set; }
        public int UnitsPerBox { get; set; }
        public DateTime PurchaseTimestamp { get; set; }
        public int CombinedStockBeforePurchase { get; set; }
        public int InMovementBeforePurchase { get; set; }
        public int ExpectedCombinedStock { get; set; }
        public float BoxPrice { get; set; }

        public bool IsTimedOut(double timeoutSeconds)
        {
            return (DateTime.UtcNow - PurchaseTimestamp).TotalSeconds > timeoutSeconds;
        }
    }
}
