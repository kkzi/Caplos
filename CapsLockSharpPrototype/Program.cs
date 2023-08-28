using CapsLockSharpPrototype.Runtime;
using System;
using System.Windows.Forms;

namespace CapsLockSharpPrototype
{
    static class Program
    {
        public static Controller GlobalController;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            var flag = false;
            new System.Threading.Mutex(true, "Caplos App", out flag);
            if (!flag)
            {
                MessageBox.Show("Caplos App 已启动", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Environment.Exit(0);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
