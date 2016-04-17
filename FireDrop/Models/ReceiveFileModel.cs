using System;

namespace FireDrop.Models
{
    internal class ReceiveFileModel
    {
        private FireDropBeacon _fireDropBeacon;

        public ReceiveFileModel()
        {
            _fireDropBeacon = new FireDropBeacon();
        }

        public void StartReceiving(string receiveIp)
        {
            _fireDropBeacon.ConnectToServer(receiveIp);
        }

        public void StopReceiving()
        {
            throw new System.NotImplementedException();
        }
    }
}