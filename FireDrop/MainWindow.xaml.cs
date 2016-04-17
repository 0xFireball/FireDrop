using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;

namespace FireDrop
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IView
    {
        private bool forceClose = false;
        public MainWindow()
        {
            InitializeComponent();
            var vm = DataContext as ApplicationViewModel;
            vm.View = this;

            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
            ni.Icon = Properties.Resources.notify_icon;
            ni.Visible = true;
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
            var exitMenuItem = new MenuItem() {Text = "Exit"};
            exitMenuItem.Click +=
                delegate(object sender, EventArgs args)
                {
                    forceClose = true;
                    this.Close();
                };
            ni.ContextMenu = new ContextMenu(new MenuItem[] { exitMenuItem });
        }

        public Window WindowHandle => this;

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = !forceClose;
            this.Hide();
        }
    }
}