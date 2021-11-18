using System;

#nullable enable

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    internal sealed class WindowFrameEvent
    {
        private static int _nextSerialNumber;

        public WindowFrameEvent(string text)
        {
            SerialNumber = ++_nextSerialNumber;

            // We use DateTime.Now instead of DateTime.UtcNow because it is more useful in the view
            Time = DateTime.Now;
            Text = text;
        }

        public int SerialNumber { get; }

        public DateTime Time { get; }

        public string Text { get; }
    }
}
