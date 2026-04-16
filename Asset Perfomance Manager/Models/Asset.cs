namespace AssetPerformanceManager.Models
{
    public class Asset
    {
        public int AssetID { get; set; }
        public string Ticker { get; set; }
        public string AssetName { get; set; }
        public decimal CurrentPrice { get; set; }
    }
}