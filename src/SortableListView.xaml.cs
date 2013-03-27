using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// A ListView which sorts the columns when the column header is clicked
    /// </summary>
    public partial class SortableListView : ListView
    {
        private GridViewColumnHeader currentSortColumnHeader;
        private ListSortDirection lastSortDirection = ListSortDirection.Ascending;

        public SortableListView()
        {
            InitializeComponent();
        }

        private void ColumnHeaderClicked(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;
            if (header == null || header.Role == GridViewColumnHeaderRole.Padding)
            {
                return;
            }

            ListSortDirection direction;
            if (header != currentSortColumnHeader)
            {
                direction = ListSortDirection.Ascending;
            }
            else
            {
                if (lastSortDirection == ListSortDirection.Ascending)
                {
                    direction = ListSortDirection.Descending;
                }
                else
                {
                    direction = ListSortDirection.Ascending;
                }
            }

            Sort(header, direction);

            header.Column.HeaderTemplate = GetTemplateForSortDirection(direction);

            // Remove arrow from previously sorted header
            if (currentSortColumnHeader != null && this.currentSortColumnHeader != header)
            {
                this.currentSortColumnHeader.Column.HeaderTemplate = null;
            }

            this.currentSortColumnHeader = header;
            this.lastSortDirection = direction;
        }

        private DataTemplate GetTemplateForSortDirection(ListSortDirection direction)
        {
            string key = (direction == ListSortDirection.Ascending) ? "HeaderTemplateArrowUp" : "HeaderTemplateArrowDown";
            return this.FindResource(key) as DataTemplate;
        }

        private void Sort(GridViewColumnHeader header, ListSortDirection direction)
        {
            ICollectionView dataView =
              CollectionViewSource.GetDefaultView(this.ItemsSource);

            if (dataView != null)
            {
                dataView.SortDescriptions.Clear();
                SortDescription sd = new SortDescription(SortFieldFromColumn(header), direction);
                dataView.SortDescriptions.Add(sd);
                dataView.Refresh();
            }
        }

        /// <summary>
        /// Given a GridColumnHeader, determine the name of the field to use for sorting.
        /// The header's Tag is examined first, then the column's DisplayMemberBindng.
        /// Finally, the header's content (as a string) is used.
        /// </summary>
        /// <param name="header">The header of the column being sorted</param>
        /// <returns>The name of the field</returns>
        static string SortFieldFromColumn(GridViewColumnHeader header)
        {
            // If the header has a Tag, use it as the field name
            string field = header.Tag as string;
            if(!string.IsNullOrEmpty(field))
            {
                return field;
            }

            // If the header's column has a DisplayMemberBinding, then use that
            Binding binding = header.Column.DisplayMemberBinding as Binding;
            if (binding != null)
            {
                return binding.Path.Path;
            }
            
            // Fall back to the header's content
            return header.ToString();
        }
    }
}
