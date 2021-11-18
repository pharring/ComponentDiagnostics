using System.Windows;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    public class UIContextInformation : DependencyObject
    {
        public static DependencyProperty EnabledProperty = DependencyProperty.Register("Enabled", typeof(bool), typeof(UIContextInformation));

        public UIContextInformation(uint id, string contextName, string contextGuidString, string packageGuidString)
        {
            ID = id;
            Name = contextName;
            Guid = contextGuidString;
            Package = packageGuidString;
        }

        public override string ToString()
        {
            return string.Format("ID={0}, Guid={1}, Description={2}", ID, Guid, Name);
        }

        public uint ID { get; set; }
        public string Name { get; set; }
        public string Guid { get; set; }
        public string Package { get; set; }
        public bool Enabled
        {
            get
            {
                return (bool)GetValue(EnabledProperty);
            }
            set
            {
                SetValue(EnabledProperty, value);
            }
        }
    }
}
