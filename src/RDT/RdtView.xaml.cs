using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// Interaction logic for RdtView.xaml
    /// </summary>
    public partial class RdtView : UserControl
    {
        #region Dependency properties

        #region AutoScroll

        public static readonly DependencyProperty AutoScrollProperty = 
            DependencyProperty.Register("AutoScroll",
                                        typeof(bool),
                                        typeof(RdtView),
                                        new UIPropertyMetadata(true));

        public bool AutoScroll
        {
            get { return (bool) GetValue(AutoScrollProperty); }
            set { SetValue(AutoScrollProperty, value); }
        }

        #endregion AutoScroll

        #endregion Dependency properties

        RdtEventSink _sink;

        public RdtView()
        {
            InitializeComponent();

            this.DataContextChanged += OnDataContextChanged;

            INotifyCollectionChanged notify = (INotifyCollectionChanged) EventList.Items;
            notify.CollectionChanged += OnEventListChanged;
        }

        void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _sink = new RdtEventSink(DataContext as RdtDiagnosticsDataSource);
            _sink.Advise();
        }

        void OnEventListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // we only care about Adds
            if (e.Action != NotifyCollectionChangedAction.Add)
                return;

            // ... and then, only if we're auto-scrolling
            if (!AutoScroll)
                return;

            ItemCollection items = EventList.Items;
            EventList.ScrollIntoView(items[items.Count - 1]);
        }

        private void OnClearClicked(object sender, RoutedEventArgs e)
        {
            if (!(DataContext is RdtDiagnosticsDataSource ds))
                return;

            ds.ClearEvents();
        }
    }
}
