using System;
using System.Drawing;
using System.Windows.Forms;
using AssetPerformanceManager.Services;

namespace AssetPerformanceManager.UI
{
    public class RegisterForm : Form
    {
        private AuthService _auth = new AuthService();
        private TextBox txtUser, txtPass, txtFullName;

        public RegisterForm()
        {
            this.Text = "Регистрация нового пользователя";
            this.Size = new Size(350, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(25, 25, 25);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            InitializeUI();
        }

        private void InitializeUI()
        {
            Label lbl = new Label
            {
                Text = "СОЗДАТЬ АККАУНТ",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(50, 30),
                AutoSize = true,
                ForeColor = Color.DodgerBlue
            };

            // Поля ввода
            AddInput("Придумайте Логин:", 50, 90, out txtUser);
            AddInput("Придумайте Пароль:", 50, 170, out txtPass, true);
            AddInput("Ваше полное имя (ФИО):", 50, 250, out txtFullName);

            Button btnReg = new Button
            {
                Text = "ЗАРЕГИСТРИРОВАТЬСЯ",
                Location = new Point(50, 340),
                Size = new Size(240, 45),
                BackColor = Color.FromArgb(0, 122, 204),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnReg.Click += BtnReg_Click;

            this.Controls.AddRange(new Control[] { lbl, btnReg });
        }

        private void AddInput(string label, int x, int y, out TextBox tb, bool isPass = false)
        {
            Label l = new Label { Text = label, Location = new Point(x, y), AutoSize = true, ForeColor = Color.Gray };
            tb = new TextBox
            {
                Location = new Point(x, y + 25),
                Width = 240,
                PasswordChar = isPass ? '*' : '\0',
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(l);
            this.Controls.Add(tb);
        }

        private void BtnReg_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrWhiteSpace(txtPass.Text))
            {
                MessageBox.Show("Логин и пароль не могут быть пустыми!");
                return;
            }

            if (_auth.Register(txtUser.Text, txtPass.Text, txtFullName.Text))
            {
                MessageBox.Show("Регистрация прошла успешно! Теперь вы можете войти.");
                this.Close(); // Закрываем окно регистрации
            }
            else
            {
                MessageBox.Show("Ошибка регистрации. Возможно, такой логин уже занят.");
            }
        }
    }
}