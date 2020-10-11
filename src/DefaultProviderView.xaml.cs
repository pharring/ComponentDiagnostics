using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Internal.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// Interaction logic for DefaultProviderView.xaml
    /// The DefaultProviderView is used for diagnostics providers which do not supply their
    /// own views. It hosts a Windows Forms property grid control which does a reasonable job
    /// of displaying Gel data models. Non-Gel models are displayed in a property grid.
    /// </summary>
    public partial class DefaultProviderView : UserControl
    {
        public DefaultProviderView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handler for the DataContextChanged. Update the view to show the new
        /// data. Also, if the new DataContext supports INotifyPropertyChanged, then
        /// subscribe to the PropertyChanged event so that the property grid can refresh
        /// when the data change.
        /// </summary>
        void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            bool isListeningToPropertyChanges = this.autoRefresh.IsChecked.GetValueOrDefault();

            if (isListeningToPropertyChanges)
            {
                UnsubscribeFromPropertyChangedEvents(e.OldValue);
            }

            SetTreeViewRootNode(e.NewValue);

            // If the new data context does not support INotifyPropertyChanged, then
            // auto-refresh should be disabled.
            if (e.NewValue is INotifyPropertyChanged)
            {
                this.autoRefresh.IsEnabled = true;
            }
            else
            {
                this.autoRefresh.IsEnabled = false;
                this.autoRefresh.IsChecked = false;
            }

            if (isListeningToPropertyChanges)
            {
                SubscribeToPropertyChangedEvents(e.NewValue);
            }
        }

        private void SetTreeViewRootNode(object value)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            object gelNode = GelTreeViewDataTypes.Utilities.CreateAppropriateRootNode(value);

            // Enable the "Show Gel TreeView" check box only if we have a Gel data type.
            this.showTreeViewCheckBox.IsEnabled = (gelNode != null);

            if (gelNode != null && this.showTreeViewCheckBox.IsChecked.GetValueOrDefault())
            {
                ShowGelTreeView(gelNode);
            }
            else
            {
                // Not a Gel type, so hide the TreeView
                HideGelTreeView(value);
            }
        }

        /// <summary>
        /// If the object supports ICustomTypeProvider, there is extra type information that
        /// the Windows Forms property grid cannot see. Use an adapter to convert it to
        /// ICustomTypeDescriptor instead.
        /// </summary>
        /// <param name="o">The object to adapt</param>
        /// <returns>The adapted object, or itself if it cannot be or doesn't need to be adapted.</returns>
        private static object AdaptIfNecessary(object o)
        {
            if (o is ICustomTypeDescriptor)
            {
                // No need to adapt
                return o;
            }

            if (o is ICustomTypeProvider typeProvider)
            {
                return new CustomTypeProviderAdapter(typeProvider);
            }

            // Use the object directly. If it's a managed object, the property
            // grid will use reflection to show it.
            return o;
        }

        private void SetSelectedObject(object value)
        {
            this.innerGrid.SelectedObject = AdaptIfNecessary(value);
        }

        private void ShowGelTreeView(object gelNode)
        {
            object[] doc = { gelNode };
            this.tree.DataContext = doc;
            this.tree.Visibility = System.Windows.Visibility.Visible;
            this.splitter.Visibility = System.Windows.Visibility.Visible;

            Grid.SetRow(this.gridHost, 2);
            Grid.SetRowSpan(this.gridHost, 1);

            SetSelectedObject(gelNode);
        }

        private void HideGelTreeView(object value)
        {
            this.tree.Visibility = System.Windows.Visibility.Collapsed;
            this.splitter.Visibility = System.Windows.Visibility.Collapsed;
            this.tree.DataContext = null;

            // Move the grid into row 1 and span both rows
            Grid.SetRow(this.gridHost, 1);
            Grid.SetRowSpan(this.gridHost, 2);

            SetSelectedObject(value);
        }

        void Refresh()
        {
            if (this.tree != null && this.tree.Items != null)
            {
                this.tree.Items.Refresh();
            }
            if (this.innerGrid != null)
            {
                this.innerGrid.Refresh();
            }
        }

        void autoRefresh_Checked(object sender, RoutedEventArgs e)
        {
            // Also force an immediate refresh in case the data are stale
            Refresh();
            SubscribeToPropertyChangedEvents(this.DataContext);
        }

        void autoRefresh_Unchecked(object sender, RoutedEventArgs e)
        {
            UnsubscribeFromPropertyChangedEvents(this.DataContext);
        }

        /// <summary>
        /// If a property value changes in the data source, then refresh
        /// the property grid.
        /// </summary>
        void DataSource_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Refresh();
        }

        void SubscribeToPropertyChangedEvents(object source)
        {
            if (source is INotifyPropertyChanged notifier)
            {
                notifier.PropertyChanged += DataSource_PropertyChanged;
            }
        }

        void UnsubscribeFromPropertyChangedEvents(object source)
        {
            if (source is INotifyPropertyChanged notifier)
            {
                notifier.PropertyChanged -= DataSource_PropertyChanged;
            }
        }

        void RefreshNow_Clicked(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SetSelectedObject(e.NewValue);
        }

        private void showTreeViewCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            if (this.IsLoaded)
            {
                ShowGelTreeView(GelTreeViewDataTypes.Utilities.CreateAppropriateRootNode(this.DataContext));
            }
        }

        private void showTreeViewCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
            {
                HideGelTreeView(this.DataContext);
            }
        }
    }
}
