using AutosarGuiEditor.Source.Autosar.Events;
using AutosarGuiEditor.Source.Autosar.OsTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AutosarGuiEditor.Source.Controllers
{
    public class MoveEventsToTasksController
    {
        DataGrid EventsGrid;
        DataGrid TasksEventsGrid;
        ComboBox taskCombobox;
        Button moveLeftButton;
        Button moveRightButton;
        Button moveAllRightButton;

        AutosarEventInstancesList allFreeEvents;
        OsTask currentTask = null;

        public MoveEventsToTasksController(DataGrid EventsGrid, DataGrid TasksEventsGrid, ComboBox taskCombobox, Button moveLeftButton, Button moveRightButton, Button moveAllRightButton)
        {
            this.EventsGrid = EventsGrid;
            this.TasksEventsGrid = TasksEventsGrid;
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
            for(int i = 0; i < EventsGrid.Items.Count; i ++)
            {
                currentTask.Events.Add(allFreeEvents[i]);
            }
            ReenumerateEvents();
            UpdateTaskEvents();
            UpdateGrids();              
        }

        void moveLeftButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (currentTask != null)
            {
                int index = TasksEventsGrid.SelectedIndex;
                if ((index >= 0) && (index <= TasksEventsGrid.Items.Count - 1))
                {
                    currentTask.Events.RemoveAt(index);
                    UpdateGrids();
                }
            }
        }
        

        void moveRightButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (currentTask != null)
            {
                int index = EventsGrid.SelectedIndex;
                if ((index >= 0) && (index <= EventsGrid.Items.Count - 1))
                {
                    ReenumerateEvents();
                    allFreeEvents[index].StartupOrder = currentTask.Events.Count;
                    currentTask.Events.Add(allFreeEvents[index]);
                    UpdateGrids();
                }
            }
        }

        
        void taskCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (taskCombobox.SelectedItem is OsTask)
            {
                currentTask = (OsTask)taskCombobox.SelectedItem;
                UpdateTaskEvents();
            }
        }

        public void UpdateAllOsTasksList()
        {
            taskCombobox.ItemsSource = AutosarApplication.GetInstance().OsTasks;
        }

        public void UpdateAllEventsGrid()
        {
            allFreeEvents = AutosarApplication.GetInstance().GetAllUnnassignedEvents();
            allFreeEvents.SortByName();
            EventsGrid.ItemsSource = null;
            EventsGrid.ItemsSource = allFreeEvents;
        }

        public void UpdateTaskEvents()
        {
            if (currentTask != null)
            {
                TasksEventsGrid.ItemsSource = null;
                TasksEventsGrid.ItemsSource = currentTask.Events;
            }
        }

        public void UpdateGrids()
        {
            UpdateAllEventsGrid();
            UpdateTaskEvents();
        }

        public void ReenumerateEvents()
        {
            for (int i = 0; i < currentTask.Events.Count; i++)
            {
                currentTask.Events[i].StartupOrder = i;
            }
        }

        public void OsTaskEventOrder_ButtonDown_Click()
        {
            if (currentTask != null)
            {
                int index = TasksEventsGrid.SelectedIndex;
                if ((index >= 0) && (index < TasksEventsGrid.Items.Count - 1))
                {
                    AutosarEventInstance eventInstance = currentTask.Events[index];
                    currentTask.Events.RemoveAt(index);
                    currentTask.Events.Insert(index + 1, eventInstance);
                    ReenumerateEvents();
                    UpdateTaskEvents();
                }
            }
        }

        public void OsTaskEventsOrder_ButtonUp_Click()
        {
            if (currentTask != null)
            {
                int index = TasksEventsGrid.SelectedIndex;
                if ((index > 0) && (index < TasksEventsGrid.Items.Count))
                {
                    AutosarEventInstance eventInstance = currentTask.Events[index];
                    currentTask.Events.RemoveAt(index);
                    currentTask.Events.Insert(index - 1, eventInstance);
                    ReenumerateEvents();
                    UpdateTaskEvents();
                }
            }
        }
    }
}
