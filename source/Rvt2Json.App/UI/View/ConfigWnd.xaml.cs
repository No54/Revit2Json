using Autodesk.Revit.UI;
using System.Windows;

namespace Rvt2Json.App.UI.View
{
    public partial class ConfigWnd : Window
    {
        public ConfigWnd()
        {
            InitializeComponent();
        }

        private void OnClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnConfirm(object sender, RoutedEventArgs e)
        {
            if (!(bool)InstanceCbx.IsChecked && !(bool)TypeCbx.IsChecked)
            {
                TaskDialog.Show("Warning", "Select Instance,Type");
            }
            else
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
