using System;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// Represents a single event raised by the Running Document Table
    /// </summary>
    class RdtEvent
    {
        static int _nextSerialNumber;

        readonly int      _serialNumber;
        readonly DateTime _time;
        readonly string   _text;

        public RdtEvent (string text)
        {
            _serialNumber = ++_nextSerialNumber;
            _time         = DateTime.Now;
            _text         = text;
        }

        public int      SerialNumber    { get { return _serialNumber; } }
        public DateTime Time            { get { return _time; } }
        public string   Text            { get { return _text; } }
    }
}
