using System.Drawing;
using System.Windows.Forms;

namespace AssetPerformanceManager.UI.Components
{
    public class StyledButton : Button
    {
        public StyledButton(string text, Color backColor)
        {
            this.Text = text;
            this.FlatStyle = FlatStyle.Flat;
            this.BackColor = backColor;
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            this.Size = new Size(210, 45); // Немного шире
            this.TextAlign = ContentAlignment.MiddleLeft; // Текст слева
            this.Padding = new Padding(15, 0, 0, 0); // Отступ для текста
            this.Cursor = Cursors.Hand;
            this.FlatAppearance.BorderSize = 0;

            // Эффект при наведении
            this.MouseEnter += (s, e) => this.BackColor = ControlPaint.Light(backColor, 0.2f);
            this.MouseLeave += (s, e) => this.BackColor = backColor;
        }
    }
}
