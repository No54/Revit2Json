using Autodesk.Revit.UI;
using System.Windows;

namespace Rvt2Json.App.UI.View
{
    public partial class ConfigWnd : Window
    {
        public bool InstanceChecked;
        public bool TypeChecked;
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
                InstanceChecked = (bool)InstanceCbx.IsChecked;
                TypeChecked = (bool)TypeCbx.IsChecked;
                DialogResult = true;
                Close();
            }
        }
    }
}
