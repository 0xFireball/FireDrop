using System.Windows;
using System.Windows.Input;
using FireDrop.Models;

namespace FireDrop.ViewModels
{
    internal class ShareFileViewModel : ObservableObject
    {
        public ICommand SelectFileCommand => new DelegateCommand(SelectFile, CanSelectFile);

        public ICommand ShareFileCommand => new DelegateCommand(ShareFile, CanShareFile);

        public ICommand CancelShareCommand => new DelegateCommand(CancelShare, CanCancelShare);

        // ReSharper disable once ConvertToAutoProperty
        // ReSharper disable once InconsistentNaming
        public TextMessage ShareIP
        {
            get { return _shareIP; }
            set { _shareIP = value; }
        }

        private readonly ShareFileModel _shareFileModel;
        private bool _isSharing = false;

        // ReSharper disable once InconsistentNaming
        private TextMessage _shareIP;

        public ShareFileViewModel()
        {
            _shareFileModel = new ShareFileModel();
            _shareIP = new TextMessage();
        }

        private void CancelShare(object obj)
        {
            _shareFileModel.StopSharing();
            _isSharing = false;
            _shareIP.MessageText = "";
        }

        private void ShareFile(object o)
        {
            _shareFileModel.ShareFile();
            _isSharing = true;
            var sharingAddr = _shareFileModel.SharingIP;
            if (sharingAddr == "")
            {
                sharingAddr = "[Network Unavailable]";
            }
            _shareIP.MessageText = sharingAddr;
            Application.Current.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
        }

        private void SelectFile(object o)
        {
            _shareFileModel.SelectFileToShare();
            Application.Current.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
        }

        private bool CanShareFile(object o) => _shareFileModel.SelectedFile != null && !_isSharing;

        private bool CanSelectFile(object obj) => !_isSharing;

        private bool CanCancelShare(object obj) => _isSharing;

        public void EmergencyKill()
        {
            if (_isSharing)
                CancelShare(null);
        }
    }
}