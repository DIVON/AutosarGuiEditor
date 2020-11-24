using AutosarGuiEditor.Source.Autosar.OsTasks;
using AutosarGuiEditor.Source.Fabrics;
using AutosarGuiEditor.Source.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AutosarGuiEditor.Source.Controllers
{
    public class OsTaskEditController
    {
        Button addOsTaskButton;
        DataGrid osTaskGrid;
        AllowUpdater allowUpdater = new AllowUpdater();

        public OsTaskEditController(Button AddOsTaskButton, DataGrid OsTaskGrid)
        {
            addOsTaskButton = AddOsTaskButton;
            osTaskGrid = OsTaskGrid;
            AddOsTaskButton.Click += AddOsTaskButton_Click;
        }

        public void editName_TextBox_TextChanged(object sender)
        {
            if (allowUpdater.IsUpdateAllowed)
            {
                int index = osTaskGrid.SelectedIndex;
                if ((index < osTaskGrid.Items.Count) && (index >= 0))
                {
                    String newName = (sender as TextBox).Text;
                    if (NameUtils.CheckComponentName(newName) == true)
                    {
                        AutosarApplication.GetInstance().OsTasks[index].Name = newName;
                    }
                }
            }
        }

        public void frequency_TextBox_TextChanged(object sender)
        {
            if (allowUpdater.IsUpdateAllowed)
            {
                int index = osTaskGrid.SelectedIndex;
                if ((index < osTaskGrid.Items.Count) && (index >= 0))
                {
                    String newFrequence = (sender as TextBox).Text;
                    double res;
                    if (double.TryParse(newFrequence, out res))
                    {
                        AutosarApplication.GetInstance().OsTasks[index].PeriodMs = res;
                    }
                }
            }
        }

        public void Stacksize_TextBox_TextChanged(object sender)
        {
            if (allowUpdater.IsUpdateAllowed)
            {
                int index = osTaskGrid.SelectedIndex;
                if ((index < osTaskGrid.Items.Count) && (index >= 0))
                {
                    String newStackSize = (sender as TextBox).Text;
                    if (NameUtils.IsNumerric(newStackSize))
                    {
                        int stackSize;
                        if (int.TryParse(newStackSize, out stackSize))
                        {
                            AutosarApplication.GetInstance().OsTasks[index].StackSizeInBytes = stackSize;
                        }
                    }
                }
            }
        }

        void AddOsTaskButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            String newtaskName = NameForNewTask();
            OsTask task = OsTaskFabric.CreateOsTask(newtaskName);
            AutosarApplication.GetInstance().OsTasks.Add(task);
            int index = AutosarApplication.GetInstance().OsTasks.Count - 1;
            UpdateGrid();
        }

        public void deleteOsTaskButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            int index = osTaskGrid.SelectedIndex;
            if ((index < osTaskGrid.Items.Count) && (index >= 0))
            {
                bool delete = MessageUtilities.AskToDelete("Do you want to delete " + (osTaskGrid.Items[index] as OsTask).Name + "?");
                if (delete == true)
                {
                    AutosarApplication.GetInstance().OsTasks.RemoveAt(index);
                    UpdateGrid();
                }
            }
        }

        String NameForNewTask()
        {
            String baseName = "OsTask";
            for (int i = 0; i < 100; i++)
            {
                if (AutosarApplication.GetInstance().OsTasks.FindObject(baseName + i.ToString()) == null)
                {
                    return baseName + i.ToString();
                }
            }
            return baseName;
        }

        public void OsTaskOrderUp_Button_Click(object sender, RoutedEventArgs e)
        {
            int index = osTaskGrid.SelectedIndex;
            if ((index > 0) && (index < osTaskGrid.Items.Count))
            {
                AutosarApplication.GetInstance().OsTasks[index].Priority++;
                AutosarApplication.GetInstance().OsTasks.DoSort();
                UpdateGrid();
            }
        }

        public void OsTaskOrderDown_Button_Click(object sender, RoutedEventArgs e)
        {
            int index = osTaskGrid.SelectedIndex;
            if ((index >= 0) && (index < osTaskGrid.Items.Count - 1))
            {
                if (AutosarApplication.GetInstance().OsTasks[index].Priority - 1 >= 0)
                {
                    AutosarApplication.GetInstance().OsTasks[index].Priority--;
                    AutosarApplication.GetInstance().OsTasks.DoSort();
                    UpdateGrid();
                }
            }
        }

        public void UpdateGrid(int selectedIndex = -1)
        {
            allowUpdater.StopUpdate();
            AutosarApplication.GetInstance().OsTasks.Reenumerate();
            osTaskGrid.ItemsSource = null;
            osTaskGrid.UpdateLayout();
            osTaskGrid.ItemsSource = AutosarApplication.GetInstance().OsTasks;
            if ((selectedIndex >= 0) && (selectedIndex < osTaskGrid.Items.Count))
            {
                osTaskGrid.SelectedIndex = selectedIndex;
            }
            allowUpdater.AllowUpdate();
        }
    }
}
