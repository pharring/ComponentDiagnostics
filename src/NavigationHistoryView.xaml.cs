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
            IVsUIDispatch dispatcher = DataContext as IVsUIDispatch;

            if (dispatcher != null)
            {
                object ignored;
                dispatcher.Invoke ("Clear", pvaIn: null, pvaOut: out ignored);
            }
        }
    }
}
