using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

// Подключаем наши внутренние папки
using AssetPerformanceManager.Data;
using AssetPerformanceManager.Models;
using AssetPerformanceManager.Services;
using AssetPerformanceManager.UI.Components;
using AssetPerformanceManager.Reports;

namespace AssetPerformanceManager.UI
{
    public class MainForm : Form
    {
        // --- СЕРВИСЫ ---
        private MarketService _marketService = new MarketService();
        private ApiService _apiService = new ApiService();

        // --- ЭЛЕМЕНТЫ ИНТЕРФЕЙСА (UI) ---
        private Panel sidebar;
        private Panel header;
        private Panel chartPanel;
        private Panel drawArea;
        private DataGridView grid;
        private ContextMenuStrip contextMenu;
        private ToolTip toolTip;

        // Метки Dashboard (Верхняя панель)
        private Label lblTitle;
        private Label lblTotalValue;
        private Label lblTotalROI;
        private Label lblBenchmark;

        // Кнопки (Сайдбар)
        private StyledButton btnAdd;
        private StyledButton btnPortfolio;
        private StyledButton btnHistory;
        private StyledButton btnRisks;
        private StyledButton btnExport;
        private StyledButton btnUpdate;
        private StyledButton btnCrashTest;
        private StyledButton btnManageAssets;
        private StyledButton btnDelete;

        // --- ДАННЫЕ ---
        private List<PortfolioSummary> _currentSummary;

        public MainForm()
        {
            // Увеличиваем разрешение окна для простора
            this.Text = "Asset Performance Manager Pro v1.0";
            this.Size = new Size(1650, 950);
            this.MinimumSize = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(25, 25, 25);
            this.Font = new Font("Segoe UI", 10.5f, FontStyle.Regular);

            // 1. Сначала данные
            _marketService.SeedDefaultAssets();

            // 2. Инициализация UI
            InitializeLayout();

            // 3. Создание меню
            InitializeContextMenu();

            // 4. Загрузка данных в метки
            LoadData();
        }

        private void InitializeLayout()
        {
            // === ШАГ 1: МГНОВЕННОЕ СОЗДАНИЕ ВСЕХ ОБЪЕКТОВ (Предотвращает NullReference) ===
            toolTip = new ToolTip { InitialDelay = 500 };

            lblTitle = new Label { ForeColor = Color.White, Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true };
            lblTotalValue = new Label { ForeColor = Color.White, Font = new Font("Segoe UI", 13, FontStyle.Bold), AutoSize = true };
            lblTotalROI = new Label { ForeColor = Color.LimeGreen, Font = new Font("Segoe UI", 13, FontStyle.Bold), AutoSize = true };
            lblBenchmark = new Label { ForeColor = Color.DarkGray, Font = new Font("Segoe UI", 13, FontStyle.Bold), AutoSize = true };

            btnAdd = new StyledButton("➕ Добавить сделку", Color.FromArgb(0, 122, 204));
            btnPortfolio = new StyledButton("💼 Мой портфель", Color.FromArgb(45, 45, 45));
            btnHistory = new StyledButton("📜 История сделок", Color.FromArgb(45, 45, 45));
            btnRisks = new StyledButton("🛡️ Анализ рисков", Color.FromArgb(45, 45, 45));
            btnExport = new StyledButton("📊 Выгрузить отчет", Color.FromArgb(45, 45, 45));
            btnUpdate = new StyledButton("🔄 Обновить цены", Color.FromArgb(45, 45, 45));
            btnCrashTest = new StyledButton("💥 КРИЗИС-ТЕСТ", Color.Maroon);
            btnManageAssets = new StyledButton("🛠️ Справочник", Color.FromArgb(45, 45, 45));
            btnDelete = new StyledButton("🗑️ Удалить запись", Color.Tomato);

            sidebar = new Panel { Dock = DockStyle.Left, Width = 270, BackColor = Color.FromArgb(20, 20, 20) };
            header = new Panel { Dock = DockStyle.Top, Height = 140, BackColor = Color.FromArgb(35, 35, 35) };
            grid = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.FromArgb(30, 30, 30), BorderStyle = BorderStyle.None };

            chartPanel = new Panel { Dock = DockStyle.Bottom, Height = 200, BackColor = Color.FromArgb(15, 15, 15) };
            drawArea = new Panel { Location = new Point(20, 40), Size = new Size(230, 150) };

            // === ШАГ 2: РАЗМЕЩЕНИЕ ЭЛЕМЕНТОВ ===
            this.Controls.Add(grid);
            this.Controls.Add(header);
            this.Controls.Add(sidebar);

            // --- Сайдбар ---
            Label logo = new Label
            {
                Text = "APM ПРО v1.0",
                ForeColor = Color.DodgerBlue,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                Location = new Point(25, 25),
                AutoSize = true
            };
            sidebar.Controls.Add(logo);

            int btnY = 100; int spacing = 52;

            btnAdd.Location = new Point(25, btnY);
            btnAdd.Click += (s, e) => ShowTransactionForm();
            sidebar.Controls.Add(btnAdd);

            btnPortfolio.Location = new Point(25, btnY += spacing);
            btnPortfolio.Click += (s, e) => { lblTitle.Text = "Обзор портфеля"; btnDelete.Visible = false; LoadData(); };
            sidebar.Controls.Add(btnPortfolio);

            btnHistory.Location = new Point(25, btnY += spacing);
            btnHistory.Click += (s, e) => { ShowHistory(); btnDelete.Visible = true; };
            sidebar.Controls.Add(btnHistory);

            btnRisks.Location = new Point(25, btnY += spacing);
            btnRisks.Click += (s, e) => { ShowRiskAnalytics(); btnDelete.Visible = false; };
            sidebar.Controls.Add(btnRisks);

            btnExport.Location = new Point(25, btnY += spacing);
            btnExport.Click += (s, e) => ExportReport();
            sidebar.Controls.Add(btnExport);

            btnUpdate.Location = new Point(25, btnY += spacing);
            // Исправлено: вызываем асинхронный метод напрямую
            btnUpdate.Click += async (s, e) => await UpdatePricesAsync();
            sidebar.Controls.Add(btnUpdate);

            btnCrashTest.Location = new Point(25, btnY += spacing);
            btnCrashTest.Click += (s, e) => RunStressTest();
            sidebar.Controls.Add(btnCrashTest);

            btnManageAssets.Location = new Point(25, btnY += spacing);
            btnManageAssets.Click += (s, e) => { using (var f = new AssetForm()) { if (f.ShowDialog() == DialogResult.OK) LoadData(); } };
            sidebar.Controls.Add(btnManageAssets);

            btnDelete.Location = new Point(25, btnY += spacing + 15);
            btnDelete.Visible = false;
            btnDelete.Click += (s, e) => DeleteSelectedTransaction();
            sidebar.Controls.Add(btnDelete);

            sidebar.Controls.Add(chartPanel);
            Label lblChartTitle = new Label { Text = "СТРУКТУРА АКТИВОВ:", ForeColor = Color.Gray, Font = new Font("Segoe UI", 8, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true };
            chartPanel.Controls.Add(lblChartTitle);
            drawArea.Paint += (s, e) => DrawAssetChart(e.Graphics, drawArea.Width, drawArea.Height);
            chartPanel.Controls.Add(drawArea);

            // --- Шапка (Карточки) ---
            FlowLayoutPanel metricsContainer = new FlowLayoutPanel
            {
                Location = new Point(10, 15),
                Size = new Size(1400, 120),
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true
            };
            header.Controls.Add(metricsContainer);

            // Локальная функция для создания адаптивной КРУПНОЙ карточки
            Panel CreateAdaptiveCard(string title, Label valLabel, Color accentColor)
            {
                Panel p = new Panel
                {
                    BackColor = Color.FromArgb(45, 45, 48),
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Padding = new Padding(25, 15, 45, 15),
                    Margin = new Padding(0, 0, 20, 0)
                };
                FlowLayoutPanel stack = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.TopDown,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    BackColor = Color.Transparent,
                    WrapContents = false
                };
                Label t = new Label { Text = title, ForeColor = Color.Gray, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 0, 0, 5) };
                valLabel.ForeColor = accentColor; valLabel.Margin = new Padding(0);
                stack.Controls.Add(t); stack.Controls.Add(valLabel); p.Controls.Add(stack);
                return p;
            }

            metricsContainer.Controls.Add(CreateAdaptiveCard("СТОИМОСТЬ ПОРТФЕЛЯ", lblTotalValue, Color.White));
            metricsContainer.Controls.Add(CreateAdaptiveCard("ОБЩАЯ ДОХОДНОСТЬ", lblTotalROI, Color.LimeGreen));
            metricsContainer.Controls.Add(CreateAdaptiveCard("vs ИНДЕКС (БЕНЧМАРК)", lblBenchmark, Color.Silver));

            // Поиск
            TextBox txtSearch = new TextBox
            {
                Location = new Point(1450, 25),
                Width = 150,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11),
                Text = "Поиск...",
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            txtSearch.Enter += (s, e) => { if (txtSearch.Text == "Поиск...") txtSearch.Text = ""; };
            txtSearch.TextChanged += (s, e) => FilterGrid(txtSearch.Text);
            header.Controls.Add(txtSearch);

            // --- Таблица ---
            grid.SelectionMode = DataGridViewSelectionMode.CellSelect;
            grid.MultiSelect = true;
            grid.AllowUserToAddRows = false;
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.EnableHeadersVisualStyles = false;
            grid.GridColor = Color.FromArgb(45, 45, 45);
            grid.RowTemplate.Height = 55;
            grid.KeyDown += Grid_KeyDown;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.Gainsboro;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            grid.ColumnHeadersHeight = 65;
            grid.DefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            grid.DefaultCellStyle.ForeColor = Color.White;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);

            grid.BringToFront();

            toolTip.SetToolTip(btnAdd, "Добавить операцию");
            toolTip.SetToolTip(btnUpdate, "Обновить курсы BYN и котировки");
        }

        private async Task UpdatePricesAsync()
        {
            btnUpdate.Enabled = false;
            string originalText = btnUpdate.Text;
            btnUpdate.Text = "⏳ Загрузка...";
            try
            {
                await _apiService.UpdateAllPricesFromWeb();
                LoadData();
                MessageBox.Show("Котировки успешно обновлены!");
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
            finally
            {
                btnUpdate.Enabled = true;
                btnUpdate.Text = originalText;
            }
        }

        public void LoadData()
        {
            try
            {
                if (CurrentSession.CurrentUser == null) return;
                var dataList = _marketService.GetPortfolioSummary();
                _currentSummary = dataList;

                grid.DataSource = null;
                if (grid.Columns.Count > 0) grid.Columns.Clear();

                DataTable dt = new DataTable();
                dt.Columns.Add("Тикер");
                dt.Columns.Add("Кол-во", typeof(decimal));
                dt.Columns.Add("Ср. цена", typeof(decimal));
                dt.Columns.Add("Рыночная цена", typeof(decimal));
                dt.Columns.Add("Стоимость", typeof(decimal));
                dt.Columns.Add("Прибыль/Убыток", typeof(decimal));
                dt.Columns.Add("Доходность %", typeof(decimal));

                decimal totalVal = 0; decimal totalCost = 0;
                foreach (var item in dataList)
                {
                    dt.Rows.Add(item.Ticker, item.TotalQuantity, item.AverageCost, item.CurrentMarketPrice, item.MarketValue, item.PnL, item.ROI);
                    totalVal += item.MarketValue;
                    totalCost += (item.TotalQuantity * item.AverageCost);
                }
                grid.DataSource = dt;

                if (grid.Columns.Count >= 7)
                {
                    for (int i = 2; i <= 6; i++) grid.Columns[i].DefaultCellStyle.Format = "N2";
                    grid.Columns[1].DefaultCellStyle.Format = "N4";
                }

                decimal roi = AnalyticsEngine.CalculateROI(totalCost, totalVal);
                lblTotalValue.Text = $"Стоимость: {totalVal:N2} BYN";
                lblTotalROI.Text = $"Доходность: {roi:N2}%";
                lblTotalROI.ForeColor = roi >= 0 ? Color.LimeGreen : Color.Tomato;

                decimal deviation = AnalyticsEngine.GetBenchmarkDeviation(roi);
                lblBenchmark.Text = $"vs Индекс: {(deviation >= 0 ? "+" : "")}{deviation:N2}%";
                lblBenchmark.ForeColor = deviation >= 0 ? Color.LimeGreen : Color.Tomato;

                grid.ClearSelection();
                drawArea.Invalidate();
            }
            catch (Exception) { }
        }

        private void ShowHistory()
        {
            try
            {
                grid.DataSource = null;
                grid.Columns.Clear();
                DataTable history = _marketService.GetFullTransactionHistory();
                grid.DataSource = history;
                lblTitle.Text = "История операций";
                if (grid.Columns.Contains("Кол-во")) grid.Columns["Кол-во"].DefaultCellStyle.Format = "N4";
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void ExportReport()
        {
            if (_currentSummary == null || _currentSummary.Count == 0) return;
            decimal totalVal = _currentSummary.Sum(x => x.MarketValue);
            decimal totalCost = _currentSummary.Sum(x => x.TotalQuantity * x.AverageCost);
            decimal roi = AnalyticsEngine.CalculateROI(totalCost, totalVal);
            ExportManager.ExportToText(_currentSummary, totalVal, roi);
        }

        private void DrawAssetChart(Graphics g, int width, int height)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            if (_currentSummary == null || _currentSummary.Count == 0) return;
            var categoryData = _currentSummary.GroupBy(x => GetCategoryByTicker(x.Ticker))
                .Select(grp => new { Name = grp.Key, Val = grp.Sum(s => s.MarketValue) }).ToList();
            decimal totalValue = categoryData.Sum(x => x.Val);
            if (totalValue == 0) return;
            int currentY = 0; int barH = 35;
            Color[] colors = { Color.DodgerBlue, Color.Orange, Color.MediumPurple, Color.LightSeaGreen, Color.Tomato };
            int i = 0;
            foreach (var item in categoryData)
            {
                float perc = (float)(item.Val / totalValue);
                g.FillRectangle(new SolidBrush(colors[i % colors.Length]), 0, currentY, (int)(width * perc), barH - 10);
                g.DrawString($"{item.Name}: {perc:P1}", new Font("Segoe UI", 8, FontStyle.Bold), Brushes.White, 5, currentY + 2);
                currentY += barH; i++;
            }
        }

        private string GetCategoryByTicker(string ticker)
        {
            object cat = DbHelper.ExecuteScalar("SELECT Category FROM Assets WHERE Ticker = @t", new Microsoft.Data.SqlClient.SqlParameter[] { new Microsoft.Data.SqlClient.SqlParameter("@t", ticker) });
            return cat?.ToString() ?? "Прочее";
        }

        private void ShowRiskAnalytics()
        {
            try
            {
                if (_currentSummary == null || _currentSummary.Count == 0) return;
                grid.DataSource = null; grid.Columns.Clear();
                decimal totalVal = _currentSummary.Sum(x => x.MarketValue);
                decimal totalCost = _currentSummary.Sum(x => x.TotalQuantity * x.AverageCost);
                decimal roi = AnalyticsEngine.CalculateROI(totalCost, totalVal);
                DataTable dt = new DataTable();
                dt.Columns.Add("Показатель"); dt.Columns.Add("Значение"); dt.Columns.Add("Пояснение");
                dt.Rows.Add("Коэффициент Шарпа", AnalyticsEngine.CalculateSharpeRatio(roi).ToString("N2"), "Эффективность риска");
                dt.Rows.Add("Диверсификация", AnalyticsEngine.GetDiversificationStatus(_currentSummary.Count), "Уровень распределения");
                dt.Rows.Add("Точка безубыточности", $"{totalCost:N2} BYN", "Общие затраты");
                grid.DataSource = dt;
                lblTitle.Text = "Анализ эффективности и рисков";
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void ShowTransactionForm()
        {
            using (var f = new TransactionForm()) { if (f.ShowDialog() == DialogResult.OK) LoadData(); }
        }

        private void FilterGrid(string ticker)
        {
            if (grid.DataSource is DataTable dt)
            {
                if (string.IsNullOrEmpty(ticker) || ticker == "Поиск...") dt.DefaultView.RowFilter = "";
                else dt.DefaultView.RowFilter = string.Format("Тикер LIKE '%{0}%'", ticker);
            }
        }

        private void DeleteSelectedTransaction()
        {
            try
            {
                var selectedRows = grid.SelectedCells.Cast<DataGridViewCell>().Select(c => c.OwningRow).Distinct().ToList();
                if (selectedRows.Count == 0) return;
                if (MessageBox.Show("Удалить выбранные записи?", "Удаление", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    foreach (var row in selectedRows)
                    {
                        if (lblTitle.Text == "История операций") _marketService.DeleteTransaction(Convert.ToInt32(row.Cells["ID"].Value));
                        else _marketService.DeleteAllTransactionsByTicker(row.Cells["Тикер"].Value.ToString());
                    }
                    if (lblTitle.Text == "История операций") ShowHistory(); else LoadData();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void InitializeContextMenu()
        {
            contextMenu = new ContextMenuStrip();
            ToolStripMenuItem deleteItem = new ToolStripMenuItem("Удалить запись");
            deleteItem.Click += (s, e) => DeleteSelectedTransaction();
            contextMenu.Items.Add(deleteItem);
            grid.ContextMenuStrip = contextMenu;
        }

        private void RunStressTest()
        {
            if (MessageBox.Show("Снизить рыночные цены на 25%?", "Стресс-тест", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DbHelper.ExecuteNonQuery("UPDATE Assets SET CurrentPrice = CurrentPrice * 0.75");
                LoadData();
            }
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete) { DeleteSelectedTransaction(); e.Handled = true; }
        }
    }
}