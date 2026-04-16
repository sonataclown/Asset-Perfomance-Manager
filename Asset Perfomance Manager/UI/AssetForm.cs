using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using AssetPerformanceManager.Services;
using AssetPerformanceManager.Data;

namespace AssetPerformanceManager.UI
{
    public class AssetForm : Form
    {
        private MarketService _marketService = new MarketService();
        private DataGridView assetGrid;
        private TextBox txtTicker, txtName;
        private ComboBox cbCategory;
        private NumericUpDown numPrice;

        public AssetForm()
        {
            this.Text = "Справочник активов - APM Pro";
            this.Size = new Size(600, 700);
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 10);

            InitializeUI();
            RefreshAssetList(); // Загружаем список при открытии
        }

        private void InitializeUI()
        {
            // --- ВЕРХНЯЯ ПАНЕЛЬ (ДОБАВЛЕНИЕ) ---
            Panel pnlAdd = new Panel { Dock = DockStyle.Top, Height = 250, BackColor = Color.FromArgb(35, 35, 35) };
            this.Controls.Add(pnlAdd);

            Label lblHeader = new Label { Text = "ДОБАВИТЬ НОВЫЙ ТИКЕР", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true, ForeColor = Color.DodgerBlue };
            pnlAdd.Controls.Add(lblHeader);

            // Поля ввода
            AddInput(pnlAdd, "Тикер:", out txtTicker, 20, 50, 120);
            AddInput(pnlAdd, "Название:", out txtName, 160, 50, 200);

            Label lCat = new Label { Text = "Категория:", Location = new Point(20, 110), AutoSize = true };
            cbCategory = new ComboBox { Location = new Point(20, 135), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cbCategory.Items.AddRange(new string[] { "Акции", "Криптовалюта", "Облигации", "Валюта" });
            cbCategory.SelectedIndex = 0;
            pnlAdd.Controls.AddRange(new Control[] { lCat, cbCategory });

            Label lPrice = new Label { Text = "Цена:", Location = new Point(190, 110), AutoSize = true };
            numPrice = new NumericUpDown { Location = new Point(190, 135), Width = 150, DecimalPlaces = 2, Maximum = 1000000 };
            pnlAdd.Controls.AddRange(new Control[] { lPrice, numPrice });

            Button btnAdd = new Button
            {
                Text = "СОХРАНИТЬ В БАЗУ",
                Location = new Point(20, 185),
                Size = new Size(320, 40),
                BackColor = Color.DodgerBlue,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAdd.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtTicker.Text)) return;
                _marketService.AddAsset(txtTicker.Text.ToUpper(), txtName.Text, cbCategory.SelectedItem.ToString(), numPrice.Value);
                txtTicker.Clear(); txtName.Clear();
                RefreshAssetList();
            };
            pnlAdd.Controls.Add(btnAdd);

            // --- НИЖНЯЯ ПАНЕЛЬ (ТАБЛИЦА И УДАЛЕНИЕ) ---
            assetGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(30, 30, 30),
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false
            };
            assetGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(50, 50, 50);
            assetGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            assetGrid.DefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);
            assetGrid.DefaultCellStyle.ForeColor = Color.White;

            this.Controls.Add(assetGrid);
            assetGrid.BringToFront();

            // Кнопка УДАЛИТЬ под таблицей
            Button btnDelete = new Button
            {
                Text = "УДАЛИТЬ ВЫБРАННЫЙ АКТИВ",
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.Maroon,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnDelete.Click += (s, e) => DeleteAsset();
            this.Controls.Add(btnDelete);
        }

        private void AddInput(Panel p, string label, out TextBox tb, int x, int y, int w)
        {
            p.Controls.Add(new Label { Text = label, Location = new Point(x, y), AutoSize = true });
            tb = new TextBox { Location = new Point(x, y + 25), Width = w, BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            p.Controls.Add(tb);
        }

        private void RefreshAssetList()
        {
            // Убедись, что SQL запрос верный и таблица называется Assets
            DataTable dt = DbHelper.ExecuteQuery("SELECT AssetID as [ID], Ticker as [Тикер], AssetName as [Название], Category as [Категория], CurrentPrice as [Цена] FROM Assets");
            assetGrid.DataSource = dt;
        }

        private void DeleteAsset()
        {
            if (assetGrid.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(assetGrid.SelectedRows[0].Cells["ID"].Value);
                string ticker = assetGrid.SelectedRows[0].Cells["Тикер"].Value.ToString();

                var res = MessageBox.Show($"Удалить {ticker} из справочника?", "Удаление", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (res == DialogResult.Yes)
                {
                    try
                    {
                        _marketService.DeleteAssetFromDirectory(id);
                        RefreshAssetList();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка удаления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}