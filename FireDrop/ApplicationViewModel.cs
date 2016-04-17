using System.Windows.Input;

namespace FireDrop
{
    internal class ApplicationViewModel : ObservableObject
    {
        public IView View { get; set; }
        public ICommand SwitchPageShareFileCommand => new DelegateCommand(ShareFile);

        public ICommand SwitchPageReceiveFileCommand => new DelegateCommand(ReceiveFile);

        private void ReceiveFile()
        {
            ReceiveFileWindow receiveFileWindow = new ReceiveFileWindow();
            receiveFileWindow.Owner = View.WindowHandle;
            receiveFileWindow.ShowDialog();
        }

        private void ShareFile()
        {
            ShareFileWindow shareFileWindow = new ShareFileWindow();
            shareFileWindow.Owner = View.WindowHandle;
            shareFileWindow.ShowDialog();
        }
    }
}