using System.Windows.Input;

namespace FireDrop
{
    internal class ApplicationViewModel : ObservableObject
    {
        public ICommand SwitchPageShareFileCommand => new DelegateCommand(ShareFile);

        public ICommand SwitchPageReceiveFileCommand => new DelegateCommand(ReceiveFile);

        private void ReceiveFile(object o)
        {
            var receiveFileWindow = new ReceiveFileWindow();
            receiveFileWindow.Owner = View.WindowHandle;
            receiveFileWindow.ShowDialog();
        }

        private void ShareFile(object o)
        {
            var shareFileWindow = new ShareFileWindow();
            shareFileWindow.Owner = View.WindowHandle;
            shareFileWindow.ShowDialog();
        }
    }
}