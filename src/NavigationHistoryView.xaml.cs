using System.Windows.Controls;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// Interaction logic for NavigationHistoryView.xaml
    /// </summary>
    public partial class NavigationHistoryView : DockPanel
    {
        public NavigationHistoryView()
        {
            InitializeComponent();
        }

        private void OnClearLinkClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (DataContext is IVsUIDispatch dispatcher)
            {
                _ = dispatcher.Invoke("Clear", pvaIn: null, pvaOut: out _);
            }
        }
    }
}
