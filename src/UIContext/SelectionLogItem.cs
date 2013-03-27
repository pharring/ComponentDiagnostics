using System;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    public class SelectionLogItem : SelectionItemInfo
    {
        public SelectionLogItem(bool register, VSConstants.SelectionElement selelem, string description, string ownerDescription)
            : base(selelem, description, ownerDescription)
        {
            Register = register;
        }

        public bool Register { get; set; }
        public string Event { get { return (Register) ? "In: " : "Out: "; } }
    }
}
