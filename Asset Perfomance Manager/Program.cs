using System;
using System.Windows.Forms;
using AssetPerformanceManager.UI;

namespace AssetPerformanceManager
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Для .NET Framework используем этот метод (если он доступен)
            if (Environment.OSVersion.Version.Major >= 6) SetProcessDPIAware();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var login = new AssetPerformanceManager.UI.LoginForm())
            {
                if (login.ShowDialog() == DialogResult.OK)
                {
                    Application.Run(new AssetPerformanceManager.UI.MainForm());
                }
            }
        }

        // Подключаем системную библиотеку для четкости шрифтов
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}