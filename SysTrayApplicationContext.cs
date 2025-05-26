using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace MediaController;

abstract public class SysTrayApplicationContext : ApplicationContext
    {
        private Container? _components;
        protected NotifyIcon? NotifyIcon;
        private Form? _mainForm;
        
        public SysTrayApplicationContext(bool asSysTray = true)
        {
            InitializeContext(asSysTray);
        }

        virtual protected void InitializeContext(bool asSysTray)
        {
            if (asSysTray)
            {
                _components = new Container();
                NotifyIcon = new NotifyIcon(_components);
                NotifyIcon.Visible = true;
                NotifyIcon.DoubleClick += this.notifyIcon_DoubleClick;
                NotifyIcon.ContextMenuStrip = new ContextMenuStrip();

                AddNotifyIconContextMenuItem("Open...");
                AddNotifyIconContextMenuItem("Exit");
            }
        }

        protected void AddNotifyIconContextMenuItem(String text, String? tag = null)
        {
            var tsi = NotifyIcon?.ContextMenuStrip?.Items.Add(text, null, this.contextMenuItem_Click);
            if (tsi != null)
            {
                tsi.Tag = tag == null ? text.ToUpper() : tag.ToUpper();
            }
        }

        abstract protected Form CreateMainForm();

        virtual protected void contextMenuItem_Click(Object? sender, EventArgs e)
        {
            if (sender == null) return;
            ToolStripItem tsi = (ToolStripItem)sender;
            if (tsi == null || tsi.Tag == null) return;
        
            switch (tsi.Tag.ToString()?.ToUpper())
            {
                case "EXIT":
                    Application.Exit();
                    break;

                case "OPEN":
                    OpenMainForm();
                    break;
            }
        }

        private void OpenMainForm()
        {
            if (_mainForm == null)
            {
                _mainForm = CreateMainForm();
                _mainForm.FormClosed += mainForm_FormClosed;
            }
            _mainForm.Show();
        }

        private void notifyIcon_DoubleClick(object? sender, EventArgs e)
        {
            OpenMainForm();
        }

        private void mainForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            _mainForm = null;
        }

        protected override void ExitThreadCore()
        {
            if (_mainForm != null) { _mainForm.Close(); }
            if (NotifyIcon != null) { NotifyIcon.Visible = false; } // should remove lingering tray icon!
            base.ExitThreadCore();
        }
    }
