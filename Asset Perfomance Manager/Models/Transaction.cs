using System;

namespace AssetPerformanceManager.Models
{
    public class Transaction
    {
        public int TransactionID { get; set; }
        public int AssetID { get; set; }
        public string Type { get; set; } // Buy или Sell
        public decimal Quantity { get; set; }
        public decimal PriceAtTransaction { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}