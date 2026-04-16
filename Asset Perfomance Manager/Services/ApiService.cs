using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json.Linq;
using Microsoft.Data.SqlClient;
using AssetPerformanceManager.Data;

namespace AssetPerformanceManager.Services
{
    public class ApiService
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly string[] usStocks = { "AAPL", "TSLA", "NVDA", "MSFT", "AMZN", "GOOGL", "META" };

        // Официальные курсы НБРБ (будут обновляться через API)
        private decimal _usdToByn = 3.20m; // Курс 1 USD
        private decimal _rubToByn = 3.50m; // Курс за 100 RUB

        public async Task UpdateAllPricesFromWeb()
        {
            // 1. Сначала получаем актуальные курсы валют от Нацбанка РБ
            await UpdateBynRates();

            // 2. Получаем список активов из базы
            var dt = DbHelper.ExecuteQuery("SELECT AssetID, Ticker, Category FROM Assets");

            foreach (System.Data.DataRow row in dt.Rows)
            {
                int id = Convert.ToInt32(row["AssetID"]);
                string ticker = row["Ticker"].ToString().Trim();
                string category = row["Category"].ToString().Trim();
                decimal finalPriceInByn = 0;

                try
                {
                    // А) КРИПТОВАЛЮТА (в USD) -> Конвертируем в BYN
                    if (category.Equals("Криптовалюта", StringComparison.OrdinalIgnoreCase))
                    {
                        decimal priceInUsd = await GetCryptoPrice(ticker);
                        finalPriceInByn = priceInUsd * _usdToByn;
                    }
                    // Б) АКЦИИ США (в USD) -> Конвертируем в BYN
                    else if (usStocks.Contains(ticker.ToUpper()))
                    {
                        decimal priceInUsd = await GetYahooPrice(ticker);
                        finalPriceInByn = priceInUsd * _usdToByn;
                    }
                    // В) РОССИЙСКИЕ АКТИВЫ (в RUB) -> Конвертируем в BYN
                    else
                    {
                        decimal priceInRub = await GetMoexPrice(ticker, category);
                        // Нацбанк РБ дает курс за 100 российских рублей
                        finalPriceInByn = (priceInRub / 100) * _rubToByn;
                    }

                    // 3. Записываем итоговую цену в BYN в базу данных
                    if (finalPriceInByn > 0)
                    {
                        string sql = "UPDATE Assets SET CurrentPrice = @p WHERE AssetID = @id";
                        DbHelper.ExecuteNonQuery(sql, new SqlParameter[] {
                            new SqlParameter("@p", Math.Round(finalPriceInByn, 4)), // 4 знака для точности
                            new SqlParameter("@id", id)
                        });
                    }
                }
                catch { /* Пропуск ошибок по отдельным тикерам */ }
            }
        }

        // МЕТОД ПОЛУЧЕНИЯ КУРСОВ С САЙТА НАЦБАНКА РБ
        private async Task UpdateBynRates()
        {
            try
            {
                // ID 431 - Доллар США, ID 451 - 100 Российских рублей
                string usdUrl = "https://api.nbrb.by/exrates/rates/431";
                string rubUrl = "https://api.nbrb.by/exrates/rates/451";

                var usdResponse = await client.GetStringAsync(usdUrl);
                _usdToByn = Convert.ToDecimal(JObject.Parse(usdResponse)["Cur_OfficialRate"]);

                var rubResponse = await client.GetStringAsync(rubUrl);
                _rubToByn = Convert.ToDecimal(JObject.Parse(rubResponse)["Cur_OfficialRate"]);
            }
            catch
            {
                // Если API Нацбанка недоступно, оставляем значения по умолчанию
            }
        }

        private async Task<decimal> GetMoexPrice(string ticker, string category)
        {
            try
            {
                string market = category.Equals("Облигации", StringComparison.OrdinalIgnoreCase) ? "bonds" : "shares";
                string board = category.Equals("Облигации", StringComparison.OrdinalIgnoreCase) ? (ticker.ToUpper().StartsWith("SU") ? "TQOB" : "TQCB") : "TQBR";
                string url = $"https://iss.moex.com/iss/engines/stock/markets/{market}/boards/{board}/securities/{ticker}.json?iss.meta=off&iss.only=marketdata&marketdata.columns=LAST";

                var response = await client.GetStringAsync(url);
                var data = JObject.Parse(response)["marketdata"]["data"];
                if (data != null && data.Count() > 0 && data[0][0] != null)
                {
                    decimal raw = Convert.ToDecimal(data[0][0]);
                    return category.Equals("Облигации", StringComparison.OrdinalIgnoreCase) ? raw * 10 : raw;
                }
            }
            catch { }
            return 0;
        }

        private async Task<decimal> GetYahooPrice(string ticker)
        {
            try
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                var response = await client.GetStringAsync($"https://query1.finance.yahoo.com/v8/finance/chart/{ticker}");
                return Convert.ToDecimal(JObject.Parse(response)["chart"]["result"][0]["meta"]["regularMarketPrice"]);
            }
            catch { return 0; }
        }

        private async Task<decimal> GetCryptoPrice(string ticker)
        {
            try
            {
                string coinId = ticker.ToLower() == "btc" ? "bitcoin" : (ticker.ToLower() == "eth" ? "ethereum" : ticker.ToLower());
                var response = await client.GetStringAsync($"https://api.coingecko.com/api/v3/simple/price?ids={coinId}&vs_currencies=usd");
                return Convert.ToDecimal(JObject.Parse(response)[coinId]["usd"]);
            }
            catch { return 0; }
        }
    }
}