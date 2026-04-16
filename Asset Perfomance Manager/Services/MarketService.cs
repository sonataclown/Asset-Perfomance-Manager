using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using AssetPerformanceManager.Data;
using AssetPerformanceManager.Models;

namespace AssetPerformanceManager.Services
{
    public class MarketService
    {
        /// <summary>
        /// 1. Получение сводки по портфелю ТЕКУЩЕГО пользователя
        /// </summary>
        public List<PortfolioSummary> GetPortfolioSummary()
        {
            // Всегда создаем объект списка, чтобы никогда не возвращать null
            var summaryList = new List<PortfolioSummary>();

            if (CurrentSession.CurrentUser == null) return summaryList;

            string sql = @"
        SELECT 
            A.Ticker, 
            SUM(CASE WHEN UPPER(T.Type) IN ('BUY', 'ПОКУПКА') THEN T.Quantity ELSE -T.Quantity END) as Holdings,
            A.CurrentPrice,
            CAST(SUM(CASE WHEN UPPER(T.Type) IN ('BUY', 'ПОКУПКА') THEN T.Quantity * T.PriceAtTransaction ELSE 0 END) / 
                 NULLIF(SUM(CASE WHEN UPPER(T.Type) IN ('BUY', 'ПОКУПКА') THEN T.Quantity ELSE 0 END), 0) AS DECIMAL(18,4)) as AvgCost
        FROM Assets A
        JOIN Transactions T ON A.AssetID = T.AssetID
        WHERE T.UserID = @uid
        GROUP BY A.Ticker, A.CurrentPrice
        HAVING SUM(CASE WHEN UPPER(T.Type) IN ('BUY', 'ПОКУПКА') THEN T.Quantity ELSE -T.Quantity END) > 0";

            try
            {
                DataTable dt = DbHelper.ExecuteQuery(sql, new SqlParameter[] {
            new SqlParameter("@uid", CurrentSession.CurrentUser.UserID)
        });

                if (dt == null) return summaryList;

                foreach (DataRow row in dt.Rows)
                {
                    // Используем безопасное приведение типов (Convert)
                    summaryList.Add(new PortfolioSummary
                    {
                        Ticker = row["Ticker"]?.ToString() ?? "???",
                        TotalQuantity = row["Holdings"] != DBNull.Value ? Convert.ToDecimal(row["Holdings"]) : 0,
                        AverageCost = row["AvgCost"] != DBNull.Value ? Convert.ToDecimal(row["AvgCost"]) : 0,
                        CurrentMarketPrice = row["CurrentPrice"] != DBNull.Value ? Convert.ToDecimal(row["CurrentPrice"]) : 0,
                        MarketValue = (row["Holdings"] != DBNull.Value && row["CurrentPrice"] != DBNull.Value)
                                      ? Convert.ToDecimal(row["Holdings"]) * Convert.ToDecimal(row["CurrentPrice"]) : 0,
                        PnL = 0, // Посчитаем в MainForm для надежности
                        ROI = 0
                    });
                }
            }
            catch (Exception ex)
            {
                // Вместо вылета программы пишем ошибку в отладчик
                System.Diagnostics.Debug.WriteLine("ОШИБКА SQL: " + ex.Message);
            }

            return summaryList;
        }

        /// <summary>
        /// 2. Получение полной истории сделок ТЕКУЩЕГО пользователя
        /// </summary>
        public DataTable GetFullTransactionHistory()
        {
            if (CurrentSession.CurrentUser == null) return new DataTable();

            string sql = @"
        SELECT 
            T.TransactionID as [ID],
            A.Ticker as [Тикер],
            CASE WHEN UPPER(T.Type) IN ('BUY', 'ПОКУПКА') THEN N'Покупка' ELSE N'Продажа' END as [Тип],
            T.Quantity as [Кол-во],
            T.PriceAtTransaction as [Цена],
            (T.Quantity * T.PriceAtTransaction) as [Сумма],
            T.TransactionDate as [Дата]
        FROM Transactions T
        JOIN Assets A ON T.AssetID = A.AssetID
        WHERE T.UserID = @uid
        ORDER BY T.TransactionDate DESC";

            return DbHelper.ExecuteQuery(sql, new SqlParameter[] {
        new SqlParameter("@uid", CurrentSession.CurrentUser.UserID)
    });
        }

        /// <summary>
        /// 3. Добавление сделки с привязкой к UserID
        /// </summary>
        public void AddTransaction(int assetId, string type, decimal qty, decimal price)
        {
            // 1. Проверка: вошел ли пользователь в систему?
            if (CurrentSession.CurrentUser == null)
            {
                throw new Exception("Ошибка: Пользователь не авторизован!");
            }

            int currentUserId = CurrentSession.CurrentUser.UserID;

            // 2. Логика проверки баланса при продаже
            if (type == "Sell" || type == "Продажа")
            {
                string checkSql = "SELECT SUM(CASE WHEN UPPER(Type) IN ('BUY', 'ПОКУПКА') THEN Quantity ELSE -Quantity END) " +
                                  "FROM Transactions WHERE AssetID = @aid AND UserID = @uid";

                object result = DbHelper.ExecuteScalar(checkSql, new SqlParameter[] {
            new SqlParameter("@aid", assetId),
            new SqlParameter("@uid", currentUserId)
        });

                decimal currentBalance = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                if (qty > currentBalance)
                {
                    throw new Exception($"Недостаточно активов! В наличии: {currentBalance:N2}");
                }
            }

            // 3. ЗАПИСЬ СДЕЛКИ (UserID добавляется ОДИН РАЗ)
            // Важно: в базу пишем системные 'Buy'/'Sell'
            string dbType = (type == "Покупка" || type == "Buy") ? "Buy" : "Sell";

            string insertSql = @"INSERT INTO Transactions (AssetID, UserID, Type, Quantity, PriceAtTransaction, TransactionDate) 
                         VALUES (@aid, @uid, @type, @qty, @price, GETDATE())";

            SqlParameter[] insertParams = new SqlParameter[] {
        new SqlParameter("@aid", assetId),
        new SqlParameter("@uid", currentUserId), // Вот наш @uid
        new SqlParameter("@type", dbType),
        new SqlParameter("@qty", qty),
        new SqlParameter("@price", price)
    };

            DbHelper.ExecuteNonQuery(insertSql, insertParams);

            // 4. ОБНОВЛЕНИЕ ТЕКУЩЕЙ ЦЕНЫ В СПРАВОЧНИКЕ
            // Это нужно, чтобы сразу после покупки цена в портфеле стала актуальной
            string updatePriceSql = "UPDATE Assets SET CurrentPrice = @p WHERE AssetID = @aid";
            DbHelper.ExecuteNonQuery(updatePriceSql, new SqlParameter[] {
        new SqlParameter("@p", price),
        new SqlParameter("@aid", assetId)
    });
        }

        /// <summary>
        /// 4. Удаление ОДНОЙ сделки (с проверкой прав доступа)
        /// </summary>
        public void DeleteTransaction(int transactionId)
        {
            string sql = "DELETE FROM Transactions WHERE TransactionID = @id AND UserID = @uid";
            DbHelper.ExecuteNonQuery(sql, new SqlParameter[] {
                new SqlParameter("@id", transactionId),
                new SqlParameter("@uid", CurrentSession.CurrentUser.UserID)
            });
        }

        /// <summary>
        /// 5. Удаление ВСЕЙ истории по Тикеру ТЕКУЩЕГО пользователя
        /// </summary>
        public void DeleteAllTransactionsByTicker(string ticker)
        {
            string sql = "DELETE FROM Transactions WHERE UserID = @uid AND AssetID IN (SELECT AssetID FROM Assets WHERE Ticker = @t)";
            DbHelper.ExecuteNonQuery(sql, new SqlParameter[] {
                new SqlParameter("@t", ticker),
                new SqlParameter("@uid", CurrentSession.CurrentUser.UserID)
            });
        }

        /// <summary>
        /// 6. Начальное заполнение справочника активов (Общее для всех)
        /// </summary>
        public void SeedDefaultAssets()
        {
            try
            {
                object countResult = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM Assets");
                int count = (countResult != null && countResult != DBNull.Value) ? Convert.ToInt32(countResult) : 0;

                if (count == 0)
                {
                    string sql = @"
                        INSERT INTO Assets (Ticker, AssetName, Category, CurrentPrice) VALUES 
                        (N'SBER', N'ПАО Сбербанк', N'Акции', 3.40),
                        (N'GAZP', N'ПАО Газпром', N'Акции', 1.80),
                        (N'SU26238RMFS4', N'ОФЗ 26238', N'Облигации', 8.50),
                        (N'AAPL', N'Apple Inc.', N'Акции', 215.00),
                        (N'btc', N'Bitcoin', N'Криптовалюта', 65000.00)";

                    DbHelper.ExecuteNonQuery(sql);
                }
            }
            catch (Exception ex) { Console.WriteLine("Ошибка Seed: " + ex.Message); }
        }

        /// <summary>
        /// 7. Обновление рыночных цен (Симуляция или API)
        /// </summary>
        public void UpdateMarketPrices()
        {
            Random rng = new Random();
            DataTable assets = DbHelper.ExecuteQuery("SELECT AssetID, CurrentPrice FROM Assets");

            foreach (DataRow row in assets.Rows)
            {
                decimal oldPrice = Convert.ToDecimal(row["CurrentPrice"]);
                decimal factor = (decimal)(1 + (rng.NextDouble() * 0.06 - 0.03));
                DbHelper.ExecuteNonQuery("UPDATE Assets SET CurrentPrice = @price WHERE AssetID = @id", new SqlParameter[] {
                    new SqlParameter("@price", Math.Round(oldPrice * factor, 2)),
                    new SqlParameter("@id", row["AssetID"])
                });
            }
        }

        /// <summary>
        /// 8. Добавление нового актива в справочник (Общее для всех)
        /// </summary>
        public void AddAsset(string ticker, string name, string category, decimal price)
        {
            string sql = "INSERT INTO Assets (Ticker, AssetName, Category, CurrentPrice) VALUES (@t, @n, @c, @p)";
            DbHelper.ExecuteNonQuery(sql, new SqlParameter[] {
                new SqlParameter("@t", ticker),
                new SqlParameter("@n", name),
                new SqlParameter("@c", category),
                new SqlParameter("@p", price)
            });
        }

        public void DeleteAssetFromDirectory(int assetId)
        {
            try
            {
                string sql = "DELETE FROM Assets WHERE AssetID = @id";
                DbHelper.ExecuteNonQuery(sql, new SqlParameter[] { new SqlParameter("@id", assetId) });
            }
            catch
            {
                throw new Exception("Невозможно удалить актив, так как по нему есть история сделок у пользователей.");
            }
        }
    }
}