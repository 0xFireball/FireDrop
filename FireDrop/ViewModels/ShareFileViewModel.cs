using System.Windows;
using System.Windows.Input;
using FireDrop.Models;

namespace FireDrop.ViewModels
{
    internal class ShareFileViewModel : ObservableObject
    {
        public ICommand SelectFileCommand => new DelegateCommand(SelectFile);
        public ICommand ShareFileCommand => new DelegateCommand(ShareFile, CanShareFile);

        private readonly ShareFileModel _shareFileModel;

        public ShareFileViewModel()
        {
            _shareFileModel = new ShareFileModel();
        }

        private void ShareFile(object o)
        {
        }

        private void SelectFile(object o)
        {
            _shareFileModel.SelectFileToShare();
            Application.Current.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
        }

        private bool CanShareFile(object o) => _shareFileModel.SelectedFile != null;
    }
}