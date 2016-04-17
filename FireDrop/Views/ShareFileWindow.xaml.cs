using System.ComponentModel;
using System.Windows;
using FireDrop.ViewModels;

namespace FireDrop
{
    /// <summary>
    ///     Interaction logic for ShareFileWindow.xaml
    /// </summary>
    public partial class ShareFileWindow : IView
    {
        public ShareFileWindow()
        {
            InitializeComponent();
            var vm = DataContext as ShareFileViewModel;
            vm.View = this;
        }

        private void ShareFileWindow_OnClosing(object sender, CancelEventArgs e)
        {
            (this.DataContext as ShareFileViewModel).EmergencyKill();
        }

        public Window WindowHandle => this;
    }
}