using System;
using System.Collections.Generic;
using System.IO;
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

namespace AutosarGuiEditor.Source.Forms.ConsoleForm
{
    /// <summary>
    /// Interaction logic for ConsoleForm.xaml
    /// </summary>
    public partial class ConsoleForm : Window
    {
        private ConsoleData consoleString;
        public ConsoleData ConsoleText
        {
            get
            {
                return consoleString;
            }
            set
            {

            }
        }

        public ConsoleForm()
        {
            InitializeComponent();
            consoleString = new ConsoleData();
        }

        public void SetString (String str)
        {
            consoleString.Data = str;
        }

        private void consoleForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
        
    }
}
