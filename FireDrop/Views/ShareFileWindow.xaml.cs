using System.ComponentModel;
using FireDrop.ViewModels;

namespace FireDrop
{
    /// <summary>
    ///     Interaction logic for ShareFileWindow.xaml
    /// </summary>
    public partial class ShareFileWindow
    {
        public ShareFileWindow()
        {
            InitializeComponent();
        }

        private void ShareFileWindow_OnClosing(object sender, CancelEventArgs e)
        {
            (this.DataContext as ShareFileViewModel).EmergencyKill();
        }
    }
}