using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Controllers;
using AutosarGuiEditor.Source.Painters.Components.Runables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AutosarGuiEditor.Source.Forms
{
    public partial class RunnablesOrderForm : Window
    {
        private OsTaskEditController osTaskEditController;
        private MoveRunnablesToTasksController moveController;
       
        public RunnablesOrderForm()
        {
            InitializeComponent();
            osTaskEditController = new OsTaskEditController(AddOSTask_Button, OSTasks_DataGrid);
            moveController = new MoveRunnablesToTasksController(AllFreeRunnables_DataGrid, RunnablesOrder_DataGrid, SelectedOSTask_ComboBox, MoveLeft_Button, MoveRight_Button, MoveAllRight_Button);
        }
        /*
        private void RunnablesOrderUp_Button_Click(object sender, RoutedEventArgs e)
        {
            int index = RunnablesGrid.SelectedIndex;
            if ((index > 0) && (index < RunnablesGrid.Items.Count))
            {
                RunnableInstance runnable = workedRunnables[index];
                workedRunnables.RemoveAt(index);
                workedRunnables.Insert(index - 1, runnable);
                Reenumerate();
                UpdateGrid(index - 1);
            }
        }*/

        
/*
        private void RunnablesOrderDown_Button_Click(object sender, RoutedEventArgs e)
        {
            int index = RunnablesGrid.SelectedIndex;
            if ((index >= 0) && (index < RunnablesGrid.Items.Count - 1))
            {
                RunnableInstance runnable = workedRunnables[index];
                workedRunnables.RemoveAt(index);
                workedRunnables.Insert(index + 1, runnable);
                Reenumerate();
                UpdateGrid(index + 1);
            }
        }*/

        //public void Reenumerate()
        //{
        //    for (int i = 0; i < workedRunnables.Count; i++)
        //    {
        //        workedRunnables[i].StartupOrder = (i + 1);
        //    }
        //    UpdateGrid();
        //}

        private RunnableInstancesList workedRunnables = new RunnableInstancesList();

        //public void UpdateGrid(int selectedIndex = -1)
        //{
        //    RunnablesGrid.ItemsSource = null;
        //    RunnablesGrid.ItemsSource = workedRunnables;
        //    if ((selectedIndex >= 0) && (selectedIndex < RunnablesGrid.Items.Count))
        //    {
        //        RunnablesGrid.SelectedIndex = selectedIndex;
        //    }
        //}

        public void LoadData()
        {
            workedRunnables.Clear();          
            workedRunnables.AddRange(AutosarApplication.GetInstance().GetAllRunnablesOrderedByStartup());
        }

        public void ShowForm()
        {
            LoadData();
            moveController.UpdateAllRunnablesGrid();
            osTaskEditController.UpdateGrid();
            this.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void EditOsTaskName_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
          //  osTaskEditController.editName_TextBox_TextChanged(sender, e);
        }

        private void OsTaskOrderUp_Button_Click(object sender, RoutedEventArgs e)
        {
            osTaskEditController.OsTaskOrderUp_Button_Click(sender, e);
        }

        private void OsTaskOrderDown_Button_Click(object sender, RoutedEventArgs e)
        {
            osTaskEditController.OsTaskOrderDown_Button_Click(sender, e);
        }

        private void DeleteOSTask_Button_Click(object sender, RoutedEventArgs e)
        {
            osTaskEditController.deleteOsTaskButton_Click(sender, e);
        }

        private void OSTasks_DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            e.Cancel = true;
        }

        private void EditOsTaskName_TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            osTaskEditController.editName_TextBox_TextChanged(sender);
        }

        TabItem previousSelectedTab = null;
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MappingRunnableTabItem.IsSelected)
            {
                if (previousSelectedTab != MappingRunnableTabItem)
                {
                    if (moveController != null)
                    {
                        previousSelectedTab = MappingRunnableTabItem;

                        moveController.UpdateAllOsTasksList();
                        moveController.UpdateAllRunnablesGrid();
                    }
                }
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            moveController.UpdateAllOsTasksList();
            moveController.UpdateAllRunnablesGrid();
        }

        private void OsTaskRunnableOrder_ButtonUp_Click(object sender, RoutedEventArgs e)
        {
            moveController.OsTaskRunnableOrder_ButtonUp_Click();
        }

        private void OsTaskRunnableOrder_ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            moveController.OsTaskRunnableOrder_ButtonDown_Click();
        }

        private void StackSizeTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            osTaskEditController.Stacksize_TextBox_TextChanged(sender);
        }

        private void Period(object sender, System.Windows.Input.KeyEventArgs e)
        {
           
        }

        private void PeriodTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            osTaskEditController.frequency_TextBox_TextChanged(sender);
        }
    }
}
