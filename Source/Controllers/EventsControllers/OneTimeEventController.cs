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
    public class OneTimeEventController
    {
        DataGrid evenstDataGrid;
        public AllowUpdater allowUpdater = new AllowUpdater();

        public OneTimeEventController(DataGrid evenstDataGrid)
        {
            this.evenstDataGrid = evenstDataGrid;
        }

        ApplicationSwComponentType _componentDefenition;
        public ApplicationSwComponentType ComponentDefenition
        {
            set
            {
                _componentDefenition = value;

                UpdateEventsDataGrid();
            }
            get
            {
                return _componentDefenition;
            }
        }

        public void AddEventButtonClick(object sender, RoutedEventArgs e)
        {
            if (ComponentDefenition != null)
            {
                OneTimeEvent oneTimeEvent = new OneTimeEvent();
                oneTimeEvent.Name = "OneTimingEvent_Template";
                ComponentDefenition.OneTimeEvents.Add(oneTimeEvent);

                AutosarApplication.GetInstance().UpdateEventsInComponentInstances();

                UpdateEventsDataGrid();
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
                        _componentDefenition.OneTimeEvents[index].Name = tb.Text;
                    }
                }
            }
        }

        public void DeleteEventClick(object sender, RoutedEventArgs e)
        {
            if (ComponentDefenition != null)
            {
                int index = evenstDataGrid.SelectedIndex;
                if ((index < evenstDataGrid.Items.Count) && (index >= 0))
                {
                    bool delete = MessageUtilities.AskToDelete("Do you want to delete " + _componentDefenition.OneTimeEvents[index].Name + "?");
                    if (delete == true)
                    {
                        _componentDefenition.OneTimeEvents.RemoveAt(index);
                        AutosarApplication.GetInstance().SyncronizeEvents(_componentDefenition);
                        UpdateEventsDataGrid();
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
                    OneTimeEvent oneTimeEvent = evenstDataGrid.SelectedItem as OneTimeEvent;

                    if (oneTimeEvent.Runnable != null)
                    {
                        oneTimeEvent.Name = "etiOnce_" + oneTimeEvent.Runnable.Name;
                    }
                    UpdateEventsDataGrid();
                }
            }
        }

        private void UpdateEventsDataGrid()
        {
            allowUpdater.StopUpdate();
            evenstDataGrid.ItemsSource = null;
            evenstDataGrid.ItemsSource = _componentDefenition.OneTimeEvents;
            allowUpdater.AllowUpdate();
        }

        public void SelectRunnableButton_Click(object sender, RoutedEventArgs e)
        {
            if (_componentDefenition != null)
            {
                OneTimeEvent oneTimeEvent = (evenstDataGrid.SelectedItem as OneTimeEvent);

                AllComponentRunnablesForm allRunnablesForm = new AllComponentRunnablesForm(_componentDefenition.Runnables, oneTimeEvent.Runnable);
                allRunnablesForm.ShowDialog();
                if (allRunnablesForm.DialogResult.Value == true)
                {
                    oneTimeEvent.Runnable = allRunnablesForm.SelectedRunnableDefenition;
                    UpdateEventsDataGrid();

                }
            }
        }
    }
}
