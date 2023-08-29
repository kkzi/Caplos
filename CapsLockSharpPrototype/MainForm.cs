using System;
using System.Windows.Forms;
using CapsLockSharpPrototype.Helper;
using CapsLockSharpPrototype.Runtime;
using Microsoft.Win32;

namespace CapsLockSharpPrototype
{
    public partial class MainForm : Form
    {
        public static NotifyIcon NotifyIcon { get; private set; }
        public string AppName { get; private set; } = "Caplos";

        protected override CreateParams CreateParams
        {
            get
            {
                //var Params = base.CreateParams;
                //// 避免在 Win+Tab 视图显示
                //Params.ExStyle |= 0x80;
                //return Params;
                return base.CreateParams;
            }
        }
        public MainForm()
        {
            Opacity = 0;
            InitializeComponent();
            LoadHooks();
            CheckStartWithSystem();
            TrayIcon.RefleshIcon(notifyIcon);
        }

        private void QuitMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
            Environment.Exit(0);
        }

        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            var x = new About();
            x.ShowDialog();
            x.Dispose();
        }

        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
        }

        private void MainForm_OnLoad(object sender, EventArgs e)
        {
            BeginInvoke(new Action(() =>
            {
                Hide();
                Opacity = 1;
            }));
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            e.Cancel = true;
            //notifyIcon.Visible = false;
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            //if (WindowState == FormWindowState.Minimized)
            //{
            //    ShowInTaskbar = false;
            //}
        }

        private void HelpMenuItem_Click(object sender, EventArgs e)
        {
            Show();
        }

        private void startWithSystem_CheckedChanged(object sender, EventArgs e)
        {
            var currentEnabled = AutoStart.CheckEnabled(AppName);
            if (startWithSystem.Checked == currentEnabled)
            {
                return;
            }

            if (startWithSystem.Checked)
            {
                AutoStart.Enable(AppName, "\"" + Application.ExecutablePath + "\"");
            }
            else
            {
                AutoStart.Disable(AppName);
            }
        }

        private void LoadHooks()
        {
            KeyDefRuntime.NameToFunction.Add("ShowHelpWindow", () =>
            {
                Visible = !Visible;
            });

            foreach (var item in KeyDefRuntime.SetUpKeyDefs())
            {
                keysListView.Items.Add(new ListViewItem(new[] { $"[CapsLock] + {item.SourceText}", item.TargetText }));
            }
            NotifyIcon = notifyIcon;

            Program.GlobalController = new Controller();
            Program.GlobalController.SetupKeyboardHooks((y) =>
            {
                TrayIcon.RefleshIcon(notifyIcon);
            });
        }

        private void CheckStartWithSystem()
        {
            startWithSystem.Checked = AutoStart.CheckEnabled(AppName);
        }
    }
}
