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
    public partial class PackageCostTrackerView : UserControl
    {
        public PackageCostTrackerView()
        {
            InitializeComponent();
        }

        private void ScenarioComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.TaskList.DataContext = this.ScenarioComboBox.SelectedItem;
        }
    }
}
