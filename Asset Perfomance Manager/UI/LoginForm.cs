using System;
using System.Drawing;
using System.Windows.Forms;
using AssetPerformanceManager.Services;
using AssetPerformanceManager.Models;

namespace AssetPerformanceManager.UI
{
    public class LoginForm : Form
    {
        private AuthService _auth = new AuthService();
        private TextBox txtUser, txtPass;

        public LoginForm()
        {
            this.Text = "Авторизация APM Pro";
            this.Size = new Size(350, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(25, 25, 25);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 10);

            Label lbl = new Label { Text = "ВХОД В СИСТЕМУ", Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(50, 30), AutoSize = true, ForeColor = Color.DodgerBlue };

            AddInput("Логин:", 50, 100, out txtUser);
            AddInput("Пароль:", 50, 180, out txtPass, true);

            Button btnLogin = new Button { Text = "ВОЙТИ", Location = new Point(50, 270), Size = new Size(240, 40), BackColor = Color.FromArgb(0, 122, 204), FlatStyle = FlatStyle.Flat };
            btnLogin.Click += (s, e) => {
                var user = _auth.Login(txtUser.Text, txtPass.Text);
                if (user != null)
                {
                    CurrentSession.CurrentUser = user;
                    this.DialogResult = DialogResult.OK;
                }
                else MessageBox.Show("Неверный логин или пароль!");
            };

            Button btnReg = new Button { Text = "РЕГИСТРАЦИЯ", Location = new Point(50, 320), Size = new Size(240, 40), ForeColor = Color.Gray, FlatStyle = FlatStyle.Flat };
            btnReg.Click += (s, e) => ShowRegister();

            this.Controls.AddRange(new Control[] { lbl, btnLogin, btnReg });
        }

        private void AddInput(string label, int x, int y, out TextBox tb, bool isPass = false)
        {
            this.Controls.Add(new Label { Text = label, Location = new Point(x, y), AutoSize = true });
            tb = new TextBox { Location = new Point(x, y + 25), Width = 240, PasswordChar = isPass ? '*' : '\0', BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White };
            this.Controls.Add(tb);
        }

        private void ShowRegister()
        {
            using (var regForm = new RegisterForm())
            {
                regForm.ShowDialog(); // Открываем окно регистрации поверх окна входа
            }
        }
    }
}