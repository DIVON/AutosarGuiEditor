using AutosarGuiEditor.Source.Autosar.OsTasks;
using AutosarGuiEditor.Source.Painters.Components.Runables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AutosarGuiEditor.Source.Controllers
{
    public class MoveRunnablesToTasksController
    {
        DataGrid RunnablesGrid;
        DataGrid TasksRunnablesGrid;
        ComboBox taskCombobox;
        Button moveLeftButton;
        Button moveRightButton;
        Button moveAllRightButton;

        RunnableInstancesList allFreeRunnables;
        OsTask currentTask = null;

        public MoveRunnablesToTasksController(DataGrid RunnablesGrid, DataGrid TasksRunnablesGrid, ComboBox taskCombobox, Button moveLeftButton, Button moveRightButton, Button moveAllRightButton)
        {
            this.RunnablesGrid = RunnablesGrid;
            this.TasksRunnablesGrid = TasksRunnablesGrid;
            this.taskCombobox = taskCombobox;
            this.moveLeftButton = moveLeftButton;
            this.moveRightButton = moveRightButton;
            this.moveAllRightButton = moveAllRightButton;

            moveRightButton.Click += moveRightButton_Click;
            moveLeftButton.Click += moveLeftButton_Click;
            moveAllRightButton.Click += moveAllRightButton_Click;
            taskCombobox.SelectionChanged += taskCombobox_SelectionChanged;
            UpdateAllOsTasksList();
        }

        void moveAllRightButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            for(int i = 0; i < RunnablesGrid.Items.Count; i ++)
            {
                currentTask.Runnables.Add(allFreeRunnables[i]);
            }
            ReenumerateRunnables();
            UpdateTaskRunnables();
            UpdateGrids();              
        }

        void moveLeftButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (currentTask != null)
            {
                int index = TasksRunnablesGrid.SelectedIndex;
                if ((index >= 0) && (index <= TasksRunnablesGrid.Items.Count - 1))
                {
                    currentTask.Runnables.RemoveAt(index);
                    UpdateGrids();
                }
            }
        }
        

        void moveRightButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (currentTask != null)
            {
                int index = RunnablesGrid.SelectedIndex;
                if ((index >= 0) && (index <= RunnablesGrid.Items.Count - 1))
                {
                    ReenumerateRunnables();
                    allFreeRunnables[index].StartupOrder = currentTask.Runnables.Count;
                    currentTask.Runnables.Add(allFreeRunnables[index]);
                    UpdateGrids();
                }
            }
        }

        
        void taskCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (taskCombobox.SelectedItem is OsTask)
            {
                currentTask = (OsTask)taskCombobox.SelectedItem;
                UpdateTaskRunnables();
            }
        }

        public void UpdateAllOsTasksList()
        {
            taskCombobox.ItemsSource = AutosarApplication.GetInstance().OsTasks;
        }

        public void UpdateAllRunnablesGrid()
        {
            allFreeRunnables = AutosarApplication.GetInstance().GetAllUnnassignedRunnables();
            allFreeRunnables.SortByName();
            RunnablesGrid.ItemsSource = null;
            RunnablesGrid.ItemsSource = allFreeRunnables;
        }

        public void UpdateTaskRunnables()
        {
            if (currentTask != null)
            {
                TasksRunnablesGrid.ItemsSource = null;
                TasksRunnablesGrid.ItemsSource = currentTask.Runnables;
            }
        }

        public void UpdateGrids()
        {
            UpdateAllRunnablesGrid();
            UpdateTaskRunnables();
        }

        public void ReenumerateRunnables()
        {
            for (int i = 0; i < currentTask.Runnables.Count; i++)
            {
                currentTask.Runnables[i].StartupOrder = i;
            }
        }

        public void OsTaskRunnableOrder_ButtonDown_Click()
        {
            if (currentTask != null)
            {
                int index = TasksRunnablesGrid.SelectedIndex;
                if ((index >= 0) && (index < TasksRunnablesGrid.Items.Count - 1))
                {
                    PeriodicRunnableInstance runnableInstance = currentTask.Runnables[index];
                    currentTask.Runnables.RemoveAt(index);
                    currentTask.Runnables.Insert(index + 1, runnableInstance);
                    ReenumerateRunnables();
                    UpdateTaskRunnables();
                }
            }
        }

        public void OsTaskRunnableOrder_ButtonUp_Click()
        {
            if (currentTask != null)
            {
                int index = TasksRunnablesGrid.SelectedIndex;
                if ((index > 0) && (index < TasksRunnablesGrid.Items.Count))
                {
                    PeriodicRunnableInstance runnableInstance = currentTask.Runnables[index];
                    currentTask.Runnables.RemoveAt(index);
                    currentTask.Runnables.Insert(index - 1, runnableInstance);
                    ReenumerateRunnables();
                    UpdateTaskRunnables();
                }
            }
        }
    }
}
