using AutosarGuiEditor.Source.Component;
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
using AutosarGuiEditor.Source.Forms.Controls;
using System.Collections.ObjectModel;

namespace AutosarGuiEditor.Source.Forms.AllComponentRunnables
{
    /// <summary>
    /// Interaction logic for AllComponentRunnablesForm.xaml
    /// </summary>
    public partial class AllComponentRunnablesForm : Window
    {        
        public AllComponentRunnablesForm(RunnableDefenitionsList runnables, RunnableDefenition selectedRunnable)
        {
            InitializeComponent();
            
            AllRunnablesListBox.ItemsSource = runnables;
            AllRunnablesListBox.SelectedItem = selectedRunnable;
        }

        private void NoEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            e.Cancel = true;            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedRunnableDefenition != null)
            {
                DialogResult = true;
            }
            else
            {
                DialogResult = false;
            }
        }

        public RunnableDefenition SelectedRunnableDefenition;
        
        private void AllRunnablesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = (sender as ListBox);
            SelectedRunnableDefenition = listBox.SelectedItem as RunnableDefenition;
        }
    }
}
