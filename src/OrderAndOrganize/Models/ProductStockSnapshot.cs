namespace OrderAndOrganize.Models
{
    public class ProductStockSnapshot
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int OnShelves { get; set; }
        public int InStorage { get; set; }
        public int InMovement { get; set; }
        public int PendingUnreflectedUnits { get; set; }
        public int CombinedStock => OnShelves + InStorage + InMovement;
        public int EffectiveCombinedStock => CombinedStock + PendingUnreflectedUnits;
        public int UnitsPerBox { get; set; }
        public float BoxPrice { get; set; }
        public bool IsUnlocked { get; set; }
        public bool IsOrderable { get; set; }
        public bool IsOnShoppingList { get; set; }
    }
}
