using AutosarGuiEditor.Source.Autosar.SystemErrors;
using AutosarGuiEditor.Source.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AutosarGuiEditor.Source.Forms.SystemErrorsForm
{
    /// <summary>
    /// Interaction logic for SystemErrorWindow.xaml
    /// </summary>
    public partial class SystemErrorWindow : Window
    {
        public SystemErrorWindow(AutosarApplication autosarApp)
        {
            InitializeComponent();
            this.autosarApp = autosarApp;
            UpdateGrid();           
        }

        AutosarApplication autosarApp;   

        private void DeleteError_Click(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            int index = SystemErrorsGrid.SelectedIndex;
            if ((index >= 0) && (index < SystemErrorsGrid.Items.Count) )
            {
                String errMessage = "Do you want to delete this error: " + autosarApp.SystemErrors[index].Name + "?";
                if (MessageBox.Show(errMessage, "Caution", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    autosarApp.SystemErrors.RemoveAt(index);
                    UpdateGrid();
                }                
            }
        }

        private void GridView_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            e.Cancel = true;  
        }

        private void Name_TextEdit_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox box = sender as TextBox;
            int index = SystemErrorsGrid.SelectedIndex;
            if ((index < SystemErrorsGrid.Items.Count) && (index >= 0))
            {
                if (NameUtils.CheckMacroName(box.Text))
                {
                    autosarApp.SystemErrors[index].Name = box.Text;
                }
            }
        }

        private void Value_TextEdit_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox box = sender as TextBox;
            int index = SystemErrorsGrid.SelectedIndex;
            if ((index < SystemErrorsGrid.Items.Count) && (index >= 0))
            {
                UInt32 val;
                if (UInt32.TryParse(box.Text, out val))
                {
                    autosarApp.SystemErrors[index].Value = val;
                }

            }
        }


        private void AddError_Click(object sender, RoutedEventArgs e)
        {
            SystemErrorObject newErr = new SystemErrorObject();
            autosarApp.SystemErrors.Add(newErr);
            UpdateGrid();
        }

        public void UpdateGrid()
        {
            autosarApp.SystemErrors.SortErrorsByID();
            SystemErrorsGrid.ItemsSource = null;       
            SystemErrorsGrid.ItemsSource = autosarApp.SystemErrors;
        }
    }
}
