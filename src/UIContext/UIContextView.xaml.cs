using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
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
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// Diagnostics Provider for Document Well functionality.
    /// </summary>
    [Guid("c3d83822-877b-4514-ad20-63e6edd14d0b")]
    internal class UIContextDiagnosticsProvider : IVsDiagnosticsProvider, IDisposable
    {
        SelectionData _ds;
        public object DataModel
        {
            get
            {
                if (_ds == null)
                    _ds = new SelectionData();
                return _ds;
            }
        }

        public uint Version
        {
            get { return 0; }
        }

        public void Dispose()
        {
            using (_ds)
            {
                _ds = null;
            }
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Interaction logic for UIContextView.xaml
    /// </summary>
    public partial class UIContextView : DockPanel
    {
        public UIContextView()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            InitializeComponent();
            Loaded += delegate(object sender, RoutedEventArgs e)
            {
                LiveListView.SelectionChanged += LiveListView_SelectionChanged;
                SelectionToolTabs.SelectionChanged += SelectionToolTabs_SelectionChanged;
                UpdateCommands();
            };
        }

        SelectionData SelectionData
        {
            get { return (SelectionData)this.DataContext; }
        }

        private void SelectionToolTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            UpdateCommands();
        }

        private void LiveListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            UpdateCommands();
        }

        public UIContextInformation SelectedInfo()
        {
            if (SelectionToolTabs.SelectedItem == tabUIContextLog)
            {
                if (UIContextLogListview.SelectedItem != null)
                    return ((UIContextLogItem)UIContextLogListview.SelectedItem).ContextInfo;
            }
            else if (SelectionToolTabs.SelectedItem == tabLiveContexts)
            {
                return ((UIContextInformation)LiveListView.SelectedItem);
            }
            else if (SelectionToolTabs.SelectedItem == tabFavorites)
            {
                return ((UIContextInformation)FavoritesListView.SelectedItem);
            }
            return null;
        }

        public void AddToFavoritesClicked(object sender, RoutedEventArgs e)
        {
            UIContextInformation item;

            item = SelectedInfo();
            if (item != null && !SelectionData.FavoriteContexts.Contains(item))
                SelectionData.FavoriteContexts.Insert(0, item);
        }

        public void RemoveFromFavoritesClicked(object sender, RoutedEventArgs e)
        {
            UIContextInformation item;

            item = SelectedInfo();
            if (item != null && SelectionData.FavoriteContexts.Contains(item))
                SelectionData.FavoriteContexts.Remove(item);
        }

        void ToggleStatusClicked(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            UIContextInformation item;

            item = SelectedInfo();

            if (item != null)
            {
                IVsMonitorSelection selectionMonitor = Package.GetGlobalService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
                selectionMonitor.IsCmdUIContextActive(item.ID, out var active);
                selectionMonitor.SetCmdUIContext(item.ID, (active == 0) ? 1 : 0);
            }
        }

        void ClearLogClicked(object sender, RoutedEventArgs e)
        {
            if (SelectionToolTabs.SelectedItem == tabUIContextLog)
            {
                SelectionData.UIContextLog.Clear();
            }
            else if (SelectionToolTabs.SelectedItem == tabSelectionLog)
            {
                SelectionData.SelectionLog.Clear();
            }
        }

        /// <summary>
        /// Refresh the command state when a different page is selected
        /// </summary>
        void UpdateCommands()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            addToFavoritesButton.IsEnabled = (SelectionToolTabs.SelectedItem != tabFavorites);
            removeFromFavoritesButton.IsEnabled = (SelectionToolTabs.SelectedItem == tabFavorites);
            toggleStatusButton.IsEnabled = (SelectionToolTabs.SelectedItem == tabFavorites);
            clearLogButton.IsEnabled = (SelectionToolTabs.SelectedItem == tabUIContextLog || SelectionToolTabs.SelectedItem == tabSelectionLog);

            IVsUIShell uiShell = Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell;

            uiShell.UpdateCommandUI(1);
        }

    }
}
