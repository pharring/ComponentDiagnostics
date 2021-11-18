using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// Interaction logic for FileChangeServiceView.xaml
    /// </summary>
    public partial class FileChangeServiceView : UserControl
    {
        #region Dependency properties

        #region AutoScroll

        public static readonly DependencyProperty AutoScrollProperty = 
            DependencyProperty.Register("AutoScroll", 
                                        typeof(bool), 
                                        typeof(FileChangeServiceView), 
                                        new UIPropertyMetadata(true));

        public bool AutoScroll
        {
            get { return (bool) GetValue(AutoScrollProperty); }
            set { SetValue(AutoScrollProperty, value); }
        }

        #endregion AutoScroll

        #endregion Dependency properties

        readonly List<object> _subscribedWatchers = new List<object>();

        public FileChangeServiceView()
        {
            InitializeComponent();

            INotifyCollectionChanged notifyCollectionChanged = (INotifyCollectionChanged) NotificationList.Items;
            notifyCollectionChanged.CollectionChanged += OnNotificationCollectionChanged;
        }

        void OnNotificationCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // we only care about Adds
            if (e.Action != NotifyCollectionChangedAction.Add)
                return;

            // ... and then, only if we're auto-scrolling
            if (!AutoScroll)
                return;

            ItemCollection items = NotificationList.Items;
            NotificationList.ScrollIntoView (items[items.Count - 1]);
        }

        private void OnClearClicked(object sender, RoutedEventArgs e)
        {
            dynamic ds = DataContext;
            if (ds == null)
                return;

            ds.ClearNotifications();
        }

        private void OnHelpLinkClicked(object sender, RoutedEventArgs e)
        {
            Notifications.Visibility = Visibility.Collapsed;
            HelpContent.Visibility   = Visibility.Visible;
        }

        private void OnHelpDoneLinkClicked(object sender, RoutedEventArgs e)
        {
            Notifications.Visibility = Visibility.Visible;
            HelpContent.Visibility   = Visibility.Collapsed;
        }
    }
}
