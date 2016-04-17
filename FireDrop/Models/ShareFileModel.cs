using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace FireDrop.Models
{
    class ShareFileModel
    {
        private FireDropBeacon _fireDropBeacon;

        public ShareFileModel()
        {
            _fireDropBeacon = new FireDropBeacon();
        }

        public string SelectedFile { get; private set; }

        public void SelectFileToShare()
        {
            OpenFileDialog ofd = new OpenFileDialog {Multiselect = true};
            var showDialog = ofd.ShowDialog();
            if (showDialog != null && (bool) showDialog)
            {
                SelectedFile = ofd.FileName;
            }
        }

        public void ShareFile()
        {
            _fireDropBeacon.StartServer();
            _fireDropBeacon.LoadFile(SelectedFile);
        }

        public void StopSharing()
        {
            _fireDropBeacon.KillServer();
        }

        public string SharingIP => _fireDropBeacon.SharingIP;
    }
}
