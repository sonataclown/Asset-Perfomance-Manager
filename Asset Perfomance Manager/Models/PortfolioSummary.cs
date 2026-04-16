namespace AssetPerformanceManager.Models
{
    public class PortfolioSummary
    {
        public string Ticker { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal AverageCost { get; set; }
        public decimal CurrentMarketPrice { get; set; }
        public decimal MarketValue { get; set; } // Кол-во * Текущая цена
        public decimal PnL { get; set; }         // Прибыль/Убыток
        public decimal ROI { get; set; }         // Доходность в %
    }
}