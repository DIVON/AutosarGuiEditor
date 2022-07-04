using AutosarGuiEditor.Source.Autosar.Events;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Forms.AllComponentRunnables;
using AutosarGuiEditor.Source.Forms.Controls;
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
    public class TimingEventController
    {
        AutosarTreeViewControl tree;
        DataGrid evenstDataGrid;
        public AllowUpdater allowUpdater = new AllowUpdater();

        public TimingEventController(AutosarTreeViewControl AutosarTree, DataGrid evenstDataGrid)
        {
            this.tree = AutosarTree;
            this.evenstDataGrid = evenstDataGrid;
        }

        ApplicationSwComponentType _componentDefenition;
        public ApplicationSwComponentType ComponentDefenition
        {
            set
            {
                _componentDefenition = value;

                UpdateTimingEventsDataGrid();
            }
            get
            {
                return _componentDefenition;
            }
        }

        public void AddTimingEventButtonClick(object sender, RoutedEventArgs e)
        {
            if (ComponentDefenition != null)
            {
                TimingEvent timingEvent = new TimingEvent();
                ComponentDefenition.TimingEvents.Add(timingEvent);

                AutosarApplication.GetInstance().UpdateEventsInComponentInstances();

                UpdateTimingEventsDataGrid();
                tree.UpdateAutosarTreeView(tree.SelectedItem);
            }
        }

        public void TextEdit_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ComponentDefenition != null)
            {
                int index = evenstDataGrid.SelectedIndex;
                if ((index < evenstDataGrid.Items.Count) && (index >= 0))
                {
                    TextBox tb = sender as TextBox;
                    if (NameUtils.CheckComponentName(tb.Text))
                    {
                        _componentDefenition.TimingEvents[index].Name = tb.Text;
                    }
                }
            }
        }

        public void PeriodicEvent_DeleteEventClick(object sender, RoutedEventArgs e)
        {
            if (ComponentDefenition != null)
            {
                int index = evenstDataGrid.SelectedIndex;
                if ((index < evenstDataGrid.Items.Count) && (index >= 0))
                {
                    bool delete = MessageUtilities.AskToDelete("Do you want to delete " + _componentDefenition.TimingEvents[index].Name + "?");
                    if (delete == true)
                    {
                        _componentDefenition.TimingEvents.RemoveAt(index);
                        AutosarApplication.GetInstance().SyncronizeEvents(_componentDefenition);
                        tree.UpdateAutosarTreeView(tree.SelectedItem);
                        UpdateTimingEventsDataGrid();
                    }
                }
            }
        }

        public void Event_CorrectNameClick(object sender, RoutedEventArgs e)
        {
            if (ComponentDefenition != null)
            {
                int index = evenstDataGrid.SelectedIndex;
                if ((index < evenstDataGrid.Items.Count) && (index >= 0))
                {
                    TimingEvent timingEvent = evenstDataGrid.SelectedItem as TimingEvent;

                    if (timingEvent.PeriodMs >= 0)
                    {
                        timingEvent.Name = "eti" + (timingEvent.PeriodMs * 1000).ToString() + "us_" + timingEvent.Runnable.Name;
                        UpdateTimingEventsDataGrid();
                    }
                }
            }
        }

        private void UpdateTimingEventsDataGrid()
        {
            allowUpdater.StopUpdate();
            evenstDataGrid.ItemsSource = null;
            evenstDataGrid.ItemsSource = _componentDefenition.TimingEvents;
            allowUpdater.AllowUpdate();
        }

        public void TimingEvent_SelectRunnableButton_Click(object sender, RoutedEventArgs e)
        {
            if (_componentDefenition != null)
            {
                TimingEvent timingEvent = (evenstDataGrid.SelectedItem as TimingEvent);

                AllComponentRunnablesForm allRunnablesForm = new AllComponentRunnablesForm(_componentDefenition.Runnables, timingEvent.Runnable);
                allRunnablesForm.ShowDialog();
                if (allRunnablesForm.DialogResult.Value == true)
                {
                    timingEvent.Runnable = allRunnablesForm.SelectedRunnableDefenition;
                    UpdateTimingEventsDataGrid();

                }
            }
        }

        public void UpdateFrequency(String newFrequency)
        {
            int index = evenstDataGrid.SelectedIndex;
            if ((index < evenstDataGrid.Items.Count) && (index >= 0))
            {
                double newPeriod;
                if (double.TryParse(newFrequency, out newPeriod))
                {
                    _componentDefenition.TimingEvents[index].PeriodMs = newPeriod;
                }
            }
        }
    }
}
