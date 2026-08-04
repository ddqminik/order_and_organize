namespace OrderAndOrganize.Models
{
    public class ProductOrderCandidate
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int CombinedStock { get; set; }
        public int EffectiveCombinedStock { get; set; }
        public int Threshold { get; set; }
        public int Shortage => Threshold - CombinedStock;
        public int UnitsPerBox { get; set; }
        public float BoxPrice { get; set; }
    }
}
