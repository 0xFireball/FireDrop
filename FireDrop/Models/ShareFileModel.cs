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
    }
}
