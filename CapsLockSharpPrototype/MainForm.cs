﻿using System;
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
                var Params = base.CreateParams;
                // 避免在 Win+Tab 视图显示
                Params.ExStyle |= 0x80;
                return Params;
            }
        }
        public MainForm()
        {
            InitializeComponent();
            HideWindow();

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
            ShowWindow();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            HideWindow();
            e.Cancel = true;
            //notifyIcon.Visible = false;
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                ShowInTaskbar = false;
            }
        }

        private void HelpMenuItem_Click(object sender, EventArgs e)
        {
            ShowWindow();
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

        private void HideWindow()
        {
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            Visible = false;
        }

        private void ShowWindow()
        {
            ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
            Visible = true;
            Activate();
        }
        private void LoadHooks()
        {
            KeyDefRuntime.KeyDefs = KeyDefRuntime.SetUpKeyDefs();
            foreach (var item in KeyDefRuntime.KeyDefs)
            {
                if (item.Type == KeyDefRuntime.FuncType.Replace)
                {
                    keysListView.Items.Add(new ListViewItem(new[] { $"[CapsLock] + {item.AdditionKey.ToString()}", item.ReplacingKey.ToString() }));
                }
            }
            NotifyIcon = notifyIcon;

            /*normalWheel = MouseConfig.GetMouseWheel();
            fastWheel = MouseConfig.GetMouseWheel() * 2;
            if (fastWheel < 0) fastWheel = -1;
            if (fastWheel > 20) fastWheel = 20;*/

            Program.GlobalController = new Controller();
            Program.GlobalController.SetupKeyboardHooks((x, y) =>
            {
                TrayIcon.RefleshIcon(notifyIcon);
                /*if (Control.IsKeyLocked(Keys.CapsLock))
                {
                    MouseConfig.SetMouseWheel((uint)fastWheel);
                }
                else
                {
                    MouseConfig.SetMouseWheel((uint)normalWheel);
                }*/
            });
        }

        private void CheckStartWithSystem()
        {
            startWithSystem.Checked = AutoStart.CheckEnabled(AppName);
        }
    }
}
