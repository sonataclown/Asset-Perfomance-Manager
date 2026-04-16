using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using AssetPerformanceManager.Models;

namespace AssetPerformanceManager.Reports
{
    public static class ExportManager
    {
        public static void ExportToText(List<PortfolioSummary> summary, decimal totalValue, decimal totalROI)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt",
                FileName = $"Отчет_по_портфелю_{DateTime.Now:yyyy-MM-dd}.txt",
                Title = "Сохранить отчет"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("==================================================");
                sb.AppendLine("      ОТЧЕТ ПО ЭФФЕКТИВНОСТИ ПОРТФЕЛЯ (APM)       ");
                sb.AppendLine($"      Дата формирования: {DateTime.Now:f}        ");
                sb.AppendLine("==================================================");
                sb.AppendLine();

                sb.AppendLine(string.Format("{0,-10} | {1,-10} | {2,-12} | {3,-10}", "Тикер", "Кол-во", "Стоимость", "Доход %"));
                sb.AppendLine("--------------------------------------------------");

                foreach (var item in summary)
                {
                    sb.AppendLine(string.Format("{0,-10} | {1,-10:N2} | {2,-12:N2} | {3,8:N2}%",
                        item.Ticker, item.TotalQuantity, item.MarketValue, item.ROI));
                }

                sb.AppendLine("--------------------------------------------------");
                sb.AppendLine($"ИТОГОВАЯ СТОИМОСТЬ ПОРТФЕЛЯ: {totalValue:N2} ₽");
                sb.AppendLine($"ОБЩАЯ ЭФФЕКТИВНОСТЬ:        {totalROI:N2}%");
                sb.AppendLine("==================================================");

                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Отчет успешно выгружен!", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
     }
    }
