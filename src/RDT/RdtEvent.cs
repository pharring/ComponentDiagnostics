using System;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// Represents a single event raised by the Running Document Table
    /// </summary>
    class RdtEvent
    {
        private static int _nextSerialNumber;

        public RdtEvent (string text)
        {
            SerialNumber = ++_nextSerialNumber;
            Time         = DateTime.Now;
            Text         = text;
        }

        public int SerialNumber { get; }
        public DateTime Time { get; }
        public string Text { get; }
    }
}
