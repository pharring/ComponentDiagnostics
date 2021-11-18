using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
    /// Interaction logic for TaskSchedulerServiceView.xaml
    /// </summary>
    public partial class TaskSchedulerServiceView : UserControl
    {
        public TaskSchedulerServiceView()
        {
            InitializeComponent();

            INotifyCollectionChanged notifyCollectionChanged = (INotifyCollectionChanged)TaskList.Items;
            notifyCollectionChanged.CollectionChanged += OnCollectionChanged;
        }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // we only care about Adds
            if (e.Action != NotifyCollectionChangedAction.Add)
                return;

            ItemCollection items = TaskList.Items;
            TaskList.ScrollIntoView(items[items.Count - 1]);
        }

        private void OnClearClicked(object sender, RoutedEventArgs e)
        {
            dynamic ds = DataContext;
            if (ds == null)
                return;

            ds.ClearCompletedTasks();
        }
    }
}
