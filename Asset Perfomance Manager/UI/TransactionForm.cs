using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using AssetPerformanceManager.Data;      // Для доступа к DbHelper
using AssetPerformanceManager.Services;  // Для доступа к MarketService

namespace AssetPerformanceManager.UI
{
    public class TransactionForm : Form
    {
        private MarketService _marketService = new MarketService();

        // Элементы управления
        private ComboBox cbAssets;
        private ComboBox cbType;
        private NumericUpDown numQty;
        private NumericUpDown numPrice;
        private Button btnSave;

        public TransactionForm()
        {
            // Настройки окна
            this.Text = "Добавление сделки - Asset Performance Manager";
            this.Size = new Size(380, 480);
            this.BackColor = Color.FromArgb(30, 30, 30); // Темная тема
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;

            InitializeComponents();
            LoadAssetsList();
        }

        private void InitializeComponents()
        {
            // Заголовок формы
            Label lblHeader = new Label
            {
                Text = "НОВАЯ ОПЕРАЦИЯ",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.DodgerBlue,
                Location = new Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(lblHeader);

            // 1. Выбор актива
            AddLabel("Выберите актив (Тикер):", 20, 70);
            cbAssets = new ComboBox
            {
                Location = new Point(20, 95),
                Width = 320,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            this.Controls.Add(cbAssets);

            // 2. Тип сделки (Покупка/Продажа)
            AddLabel("Тип операции:", 20, 145);
            cbType = new ComboBox
            {
                Location = new Point(20, 170),
                Width = 320,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            cbType.Items.AddRange(new string[] { "Покупка", "Продажа" });
            cbType.SelectedIndex = 0;
            this.Controls.Add(cbType);

            // 3. Количество
            AddLabel("Количество единиц:", 20, 220);
            numQty = new NumericUpDown
            {
                Location = new Point(20, 245),
                Width = 320,
                DecimalPlaces = 4,
                Maximum = 10000000,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White
            };
            this.Controls.Add(numQty);

            // 4. Цена
            AddLabel("Цена сделки (за ед.):", 20, 295);
            numPrice = new NumericUpDown
            {
                Location = new Point(20, 320),
                Width = 320,
                DecimalPlaces = 2,
                Maximum = 10000000,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White
            };
            this.Controls.Add(numPrice);

            // 5. Кнопка Сохранить
            btnSave = new Button
            {
                Text = "СОХРАНИТЬ В ПОРТФЕЛЬ",
                Location = new Point(20, 380),
                Size = new Size(320, 45),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);
        }

        private void AddLabel(string text, int x, int y)
        {
            Label lbl = new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold), // Жирный шрифт для меток
                ForeColor = Color.DarkGray
            };
            this.Controls.Add(lbl);
        }

        private void LoadAssetsList()
        {
            try
            {
                // Загружаем список доступных тикеров из таблицы Assets
                DataTable dt = DbHelper.ExecuteQuery("SELECT AssetID, Ticker FROM Assets");
                cbAssets.DataSource = dt;
                cbAssets.DisplayMember = "Ticker";
                cbAssets.ValueMember = "AssetID";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки списка активов: " + ex.Message, "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cbAssets.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите актив.");
                return;
            }

            if (numQty.Value <= 0 || numPrice.Value <= 0)
            {
                MessageBox.Show("Количество и цена должны быть больше нуля.");
                return;
            }

            try
            {
                // Переводим русский выбор обратно в системные значения для базы данных
                string typeForDb = cbType.SelectedItem.ToString() == "Покупка" ? "Buy" : "Sell";

                // Сохраняем сделку через сервис
                _marketService.AddTransaction(
                    (int)cbAssets.SelectedValue,
                    typeForDb,
                    numQty.Value,
                    numPrice.Value
                );

                MessageBox.Show("Операция успешно зарегистрирована!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить сделку: " + ex.Message, "Критическая ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}