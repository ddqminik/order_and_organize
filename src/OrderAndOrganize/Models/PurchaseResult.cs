namespace OrderAndOrganize.Models
{
    public class PurchaseResult
    {
        public bool Success { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public float BoxPrice { get; set; }
        public float MoneyBefore { get; set; }
        public float MoneyAfter { get; set; }
        public string FailureReason { get; set; }

        public static PurchaseResult Succeeded(int productId, string productName, float boxPrice, float moneyBefore, float moneyAfter)
        {
            return new PurchaseResult
            {
                Success = true,
                ProductId = productId,
                ProductName = productName,
                BoxPrice = boxPrice,
                MoneyBefore = moneyBefore,
                MoneyAfter = moneyAfter
            };
        }

        public static PurchaseResult Failed(int productId, string productName, float boxPrice, string reason)
        {
            return new PurchaseResult
            {
                Success = false,
                ProductId = productId,
                ProductName = productName,
                BoxPrice = boxPrice,
                FailureReason = reason
            };
        }
    }
}
