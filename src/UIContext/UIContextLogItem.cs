using System;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    public class UIContextLogItem
    {
        public UIContextLogItem(bool activate, UIContextInformation info)
        {
            Activate = activate;
            ContextInfo = info;
            Time = DateTime.Now;
        }

        public DateTime Time { get; set; }
        public bool Activate { get; set; }
        public UIContextInformation ContextInfo { get; set; }

        public string Event
        {
            get
            {
                return (Activate) ? "Activate: " : "Deactivate: ";
            }
        }
    }
}