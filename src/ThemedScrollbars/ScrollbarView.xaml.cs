using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// Interaction logic for ScrollbarView.xaml
    /// </summary>
    public partial class ScrollbarView : UserControl
    {
        public ScrollbarView()
        {
            InitializeComponent();
            this.DataContextChanged += OnDataContextChanged;
        }

        void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Refresh();
        }

        private void OnRefreshLinkClicked(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        void Refresh()
        {
            ScrollbarDiagnosticsDataSource ds = DataContext as ScrollbarDiagnosticsDataSource;
            if (ds == null)
                return;

            ds.Refresh();
        }
    }
}
