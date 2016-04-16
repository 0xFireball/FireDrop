using System;

namespace FlamingLeafClient
{
    internal class FlamingLeafDataReceivedEventArgs : EventArgs
    {
        public FlamingLeafDataReceivedEventArgs(string data) : base()
        {
            Data = data;
        }

        public string Data { get; private set; }
    }
}