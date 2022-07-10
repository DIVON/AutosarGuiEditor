using AutosarGuiEditor.Source.Autosar.Events;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Forms.AllComponentRunnables;
using AutosarGuiEditor.Source.Forms.ClientServerEventsForm;
using AutosarGuiEditor.Source.Forms.Controls;
using AutosarGuiEditor.Source.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AutosarGuiEditor.Source.Controllers.EventsControllers
{
    public class ClientServerEventController
    {
        AutosarTreeViewControl tree;
        DataGrid evenstDataGrid;
        public AllowUpdater allowUpdater = new AllowUpdater();

        Boolean isAsync;

        public ClientServerEventController(AutosarTreeViewControl AutosarTree, Boolean isAsync, DataGrid evenstDataGrid)
        {
            this.tree = AutosarTree;
            this.isAsync = isAsync;
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
                ClientServerEvent csEvent = new ClientServerEvent();
                csEvent.Name = "defaultEvent";
                if (isAsync == true)
                {
                    ComponentDefenition.AsyncClientServerEvents.Add(csEvent);
                }
                else
                {
                    ComponentDefenition.SyncClientServerEvents.Add(csEvent);
                }

                AutosarApplication.GetInstance().UpdateEventsInComponentInstances();

                UpdateEventsDataGrid();
            }
        }

        public void EventName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ComponentDefenition != null)
            {
                int index = evenstDataGrid.SelectedIndex;
                if ((index < evenstDataGrid.Items.Count) && (index >= 0))
                {
                    TextBox tb = sender as TextBox;
                    if (NameUtils.CheckComponentName(tb.Text))
                    {
                        ClientServerEventList list;

                        if (isAsync == true)
                        {
                            list = _componentDefenition.AsyncClientServerEvents;
                        }
                        else
                        {
                            list = _componentDefenition.SyncClientServerEvents;
                        }

                        list[index].Name = tb.Text;
                    }
                }
            }
        }

        public void DeleteEventClick(object sender, RoutedEventArgs e)
        {
            if (ComponentDefenition != null)
            {
                ClientServerEventList list;

                if (isAsync == true)
                {
                    list = _componentDefenition.AsyncClientServerEvents;
                }
                else
                {
                    list = _componentDefenition.SyncClientServerEvents;
                }

                int index = evenstDataGrid.SelectedIndex;
                if ((index < evenstDataGrid.Items.Count) && (index >= 0))
                {
                    bool delete = MessageUtilities.AskToDelete("Do you want to delete " + list[index].Name + "?");
                    if (delete == true)
                    {
                        list.RemoveAt(index);
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
                    ClientServerEvent csEvent = evenstDataGrid.SelectedItem as ClientServerEvent;

                    if ((csEvent.SourcePort != null) && (csEvent.SourceOperation != null))
                    {
                        csEvent.Name = isAsync == true ? "asyncEvent" : "syncEvent";

                        csEvent.Name += csEvent.SourcePort.Name + "_" + csEvent.SourceOperation.Name;

                        UpdateEventsDataGrid();
                    }
                }
            }
        }

        private void UpdateEventsDataGrid()
        {
            allowUpdater.StopUpdate();
            evenstDataGrid.ItemsSource = null;

            if (isAsync == true)
            {
                evenstDataGrid.ItemsSource = _componentDefenition.AsyncClientServerEvents;
            }
            else
            {
                evenstDataGrid.ItemsSource = _componentDefenition.SyncClientServerEvents;
            }
            
            allowUpdater.AllowUpdate();
        }

        public void SelectSourceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_componentDefenition != null)
            {
                ClientServerEvent csEvent = (evenstDataGrid.SelectedItem as ClientServerEvent);

                ClientServerEventSourceForm allRunnablesForm = new ClientServerEventSourceForm(_componentDefenition, isAsync);
                allRunnablesForm.ShowDialog();
                if (allRunnablesForm.DialogResult.Value == true)
                {
                    csEvent.SourceOperation = allRunnablesForm.Operation;
                    csEvent.SourcePort = allRunnablesForm.Port;

                    UpdateEventsDataGrid();
                }
            }
        }

        public void SelectRunnableButton_Click(object sender, RoutedEventArgs e)
        {
            if (_componentDefenition != null)
            {
                ClientServerEvent csEvent = (evenstDataGrid.SelectedItem as ClientServerEvent);

                AllComponentRunnablesForm allRunnablesForm = new AllComponentRunnablesForm(_componentDefenition.Runnables, csEvent.Runnable);
                allRunnablesForm.ShowDialog();
                if (allRunnablesForm.DialogResult.Value == true)
                {
                    csEvent.Runnable = allRunnablesForm.SelectedRunnableDefenition;
                    UpdateEventsDataGrid();
                }
            }
        }
    }
}
