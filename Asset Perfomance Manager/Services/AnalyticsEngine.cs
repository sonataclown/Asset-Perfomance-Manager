using System;

namespace AssetPerformanceManager.Services
{
    public static class AnalyticsEngine
    {
        // Расчет чистой прибыли/убытка
        public static decimal CalculatePnL(decimal totalCost, decimal currentMarketValue)
        {
            return currentMarketValue - totalCost;
        }

        // Расчет доходности в процентах (ROI)
        public static decimal CalculateROI(decimal totalCost, decimal currentMarketValue)
        {
            if (totalCost <= 0) return 0;
            return ((currentMarketValue - totalCost) / totalCost) * 100;
        }

        // --- НОВЫЕ МЕТОДЫ, КОТОРЫХ НЕ ХВАТАЛО ---

        // Упрощенный коэффициент Шарпа (Доходность / Риск)
        public static double CalculateSharpeRatio(decimal portfolioRoi)
        {
            // Безрисковая ставка (напр. ставка ЦБ 16%)
            double riskFreeRate = 16.0;
            // Условная волатильность рынка 20%
            double volatility = 20.0;

            return (double)(portfolioRoi - (decimal)riskFreeRate) / volatility;
        }

        // Оценка уровня диверсификации
        public static string GetDiversificationStatus(int assetCount)
        {
            if (assetCount <= 0) return "Данные отсутствуют";
            if (assetCount <= 2) return "Критическая (Высокий риск)";
            if (assetCount <= 5) return "Умеренная";
            return "Хорошая (Низкий риск)";
        }

        public static string GetInvestmentAdvice(decimal roi, double sharpe, int assetCount, decimal totalValue)
        {
            if (totalValue == 0) return "Портфель пуст. Добавьте первую сделку!";

            if (roi < -20)
                return "⚠️ КРИТИЧЕСКИЙ УБЫТОК: Рассмотрите возможность закрытия слабых позиций.";

            if (assetCount < 3)
                return "⚖️ НИЗКАЯ ДИВЕРСИФИКАЦИЯ: Ваш риск слишком сконцентрирован. Добавьте другие активы.";

            if (sharpe < 0 && roi > 0)
                return "📈 ПРИБЫЛЬ ПРИ ВЫСОКОМ РИСКЕ: Рекомендуется зафиксировать часть прибыли.";

            if (roi > 15)
                return "🚀 ОТЛИЧНЫЙ РЕЗУЛЬТАТ: Портфель обгоняет рынок. Сохраняйте стратегию.";

            return "✅ ПОРТФЕЛЬ СБАЛАНСИРОВАН: Продолжайте плановое удержание активов.";
        }

        // Расчет отклонения от рыночного эталона (Бенчмарка)
        public static decimal GetBenchmarkDeviation(decimal portfolioRoi)
        {
            decimal marketBenchmark = 15.0m; // Средняя доходность индекса (за год)
            return portfolioRoi - marketBenchmark;
        }
    }
}