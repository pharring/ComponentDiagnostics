using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

#nullable enable

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// Interaction logic for WindowFramesView.xaml
    /// </summary>
    public partial class WindowFramesView : UserControl
    {
        private WindowFrameEventsSink? _windowFrameEventsSink;

        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.Register("AutoScroll", typeof(bool), typeof(WindowFramesView), new UIPropertyMetadata(true));

        public bool AutoScroll
        {
            get { return (bool)GetValue(AutoScrollProperty); }
            set { SetValue(AutoScrollProperty, value); }
        }

        public static readonly DependencyProperty ShowDocumentWindowFramesProperty =
            DependencyProperty.Register("ShowDocumentWindowFrames", typeof(bool), typeof(WindowFramesView), new UIPropertyMetadata(true));

        public bool ShowDocumentWindowFrames
        {
            get { return (bool)GetValue(ShowDocumentWindowFramesProperty); }
            set { SetValue(ShowDocumentWindowFramesProperty, value); }
        }

        public static readonly DependencyProperty ShowToolWindowFramesProperty =
            DependencyProperty.Register("ShowToolWindowFrames", typeof(bool), typeof(WindowFramesView), new UIPropertyMetadata(true));

        public bool ShowToolWindowFrames
        {
            get { return (bool)GetValue(ShowToolWindowFramesProperty); }
            set { SetValue(ShowToolWindowFramesProperty, value); }
        }

        public WindowFramesView()
        {
            InitializeComponent();

            this.DataContextChanged += OnDataContextChanged;
            ((INotifyCollectionChanged)EventList.Items).CollectionChanged += OnEventListChanged;
        }

        void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (DataContext is WindowFramesDataSource windowFramesDataSource)
            {
                _windowFrameEventsSink = new WindowFrameEventsSink(windowFramesDataSource);
                _windowFrameEventsSink.Advise();
            }
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
            (DataContext as WindowFramesDataSource)?.ClearEvents();
        }

        private void ShowDocumentWindowFrames_Checked(object sender, RoutedEventArgs e)
        {
            (DataContext as WindowFramesDataSource)?.SetVisibilityOfEntries(ShowDocumentWindowFrames, ShowToolWindowFrames);
        }

        private void ShowDocumentWindowFrames_Unchecked(object sender, RoutedEventArgs e)
        {
            (DataContext as WindowFramesDataSource)?.SetVisibilityOfEntries(ShowDocumentWindowFrames, ShowToolWindowFrames);
        }

        private void ShowToolWindowFrames_Checked(object sender, RoutedEventArgs e)
        {
            (DataContext as WindowFramesDataSource)?.SetVisibilityOfEntries(ShowDocumentWindowFrames, ShowToolWindowFrames);
        }

        private void ShowToolWindowFrames_Unchecked(object sender, RoutedEventArgs e)
        {
            (DataContext as WindowFramesDataSource)?.SetVisibilityOfEntries(ShowDocumentWindowFrames, ShowToolWindowFrames);
        }
    }
}
