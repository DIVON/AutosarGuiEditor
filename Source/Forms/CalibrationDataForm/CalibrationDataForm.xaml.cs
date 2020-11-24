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

namespace AutosarGuiEditor.Source.Forms.CalibrationDataForm
{
    /// <summary>
    /// Interaction logic for CalibrationDataForm.xaml
    /// </summary>
    public partial class CalibrationDataForm : Window
    {
        public CalibrationDataForm()
        {
            InitializeComponent();
        }

        private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            e.Cancel = true;
        }

        private void CDataDescriptionNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void CDataDefenition_ChangeDatatype_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CDataDescription_DeleteButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
