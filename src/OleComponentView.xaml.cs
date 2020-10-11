using System;
using System.Windows.Controls;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// Interaction logic for OleComponentView.xaml
    /// </summary>
    public partial class OleComponentView : DockPanel, IDisposable
    {
        public OleComponentView()
        {
            InitializeComponent();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
