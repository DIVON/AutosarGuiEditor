using AutosarGuiEditor.Source.Autosar.Events;
using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Component.CData;
using AutosarGuiEditor.Source.Component.PerInstanceMemory;
using AutosarGuiEditor.Source.Controllers.EventsControllers;
using AutosarGuiEditor.Source.Forms;
using AutosarGuiEditor.Source.Forms.AllComponentRunnables;
using AutosarGuiEditor.Source.Forms.Controls;
using AutosarGuiEditor.Source.PortDefenitions;
using AutosarGuiEditor.Source.SystemInterfaces;
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
    public class ComponentDefenitionController
    {
        DataGrid portsGrid;
        DataGrid runnablesGrid;
        CheckBox multipleInstantiation_CheckBox;
        DataGrid perInstanceGrid;
        DataGrid CDataGrid;
        AutosarTreeViewControl tree;
        TextBox componentDefenitionName_TextBox;
        public AllowUpdater allowUpdater = new AllowUpdater();
        DataGrid periodicalEvenstDataGrid;
        DataGrid   syncEventDataGrid;
        DataGrid   asyncEventDataGrid;
        DataGrid   oneTimeEventDataGrid;
        public ComponentDefenitionController(
            AutosarTreeViewControl AutosarTree, 
            TextBox ComponentDefenitionName_TextBox, 
            DataGrid portsGrid, 
            DataGrid runnablesGrid, 
            CheckBox multipleInstantiation_CheckBox, 

            Button AddPerInstanceField_Button, 
            DataGrid PerInstanceGrid, 

            DataGrid CDataGrid, 
            Button AddCDataButton,
            
            DataGrid periodicalEvenstDataGrid,
           
            DataGrid   syncEventDataGrid,
            DataGrid   asyncEventDataGrid,
            DataGrid   oneTimeEventDataGrid
            )
        {
            this.tree = AutosarTree;
            this.portsGrid = portsGrid;
            this.runnablesGrid = runnablesGrid;
            this.multipleInstantiation_CheckBox = multipleInstantiation_CheckBox;
            this.perInstanceGrid = PerInstanceGrid;
            this.componentDefenitionName_TextBox = ComponentDefenitionName_TextBox;
            multipleInstantiation_CheckBox.Checked += multipleInstantiation_CheckBox_Checked;
            multipleInstantiation_CheckBox.Unchecked += multipleInstantiation_CheckBox_Unchecked;
            this.componentDefenitionName_TextBox.TextChanged += ComponentDefenitionName_TextBox_TextChanged;
            AddPerInstanceField_Button.Click += AddPerInstanceField_Button_Click;

            this.CDataGrid = CDataGrid;
            AddCDataButton.Click += AddCDataButton_Click;

            this.periodicalEvenstDataGrid = periodicalEvenstDataGrid;
            this.syncEventDataGrid = syncEventDataGrid;
            this.asyncEventDataGrid = asyncEventDataGrid;
            this.oneTimeEventDataGrid = oneTimeEventDataGrid;

            timingEventController = new TimingEventController(tree, periodicalEvenstDataGrid);
            syncEventController = new ClientServerEventController(tree, false, syncEventDataGrid);
            asyncEventController = new ClientServerEventController(tree, true, asyncEventDataGrid);
            oneTimeEventController = new OneTimeEventController(oneTimeEventDataGrid);
        }

        ClientServerEventController syncEventController;
        public ClientServerEventController SyncEventController
        {
            get
            {
                return syncEventController;
            }
        }
        ClientServerEventController asyncEventController;
        public ClientServerEventController AsyncEventController
        {
            get
            {
                return asyncEventController;
            }
        }

        OneTimeEventController oneTimeEventController;
        public OneTimeEventController OneTimeEventController
        {
            get
            {
                return oneTimeEventController;
            }
        }

        void AddCDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (_componentDefenition != null)
            {
                CDataDefenition cdataDef = new CDataDefenition();
                _componentDefenition.CDataDefenitions.Add(cdataDef);

                tree.UpdateAutosarTreeView(tree.SelectedItem);
                AutosarApplication.GetInstance().SyncronizeCData(_componentDefenition);
                UpdateCDataGrid();
            }
        }


        TimingEventController timingEventController;
        public TimingEventController TimingEvents
        {
            get
            {
                return timingEventController;
            }
        }
        void AddPerInstanceField_Button_Click(object sender, RoutedEventArgs e)
        {
            if (_componentDefenition != null)
            {
                PimDefenition perInstanceDef = new PimDefenition();
                _componentDefenition.PerInstanceMemoryList.Add(perInstanceDef);

                tree.UpdateAutosarTreeView(tree.SelectedItem);
                AutosarApplication.GetInstance().SyncronizePerInstanceMemory(_componentDefenition);
                UpdatePerInstanceMemoryGrid();
            }
        }

        void ComponentDefenitionName_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (allowUpdater.IsUpdateAllowed)
            {
                String newName = componentDefenitionName_TextBox.Text;
                if (NameUtils.CheckComponentName(newName))
                {
                    _componentDefenition.Name = newName;

                    TreeViewItem item = tree.GetItem(ComponentDefenition);
                    if (item != null)
                    {
                        item.Header = _componentDefenition.Name;
                    }
                }
            }           
        }

        void multipleInstantiation_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_componentDefenition != null)
            {
                _componentDefenition.MultipleInstantiation = false;
            }
        }

        void multipleInstantiation_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_componentDefenition != null)
            {
                _componentDefenition.MultipleInstantiation = true;
            }
        }


        ApplicationSwComponentType _componentDefenition;
        public ApplicationSwComponentType ComponentDefenition
        {
            set
            {
                _componentDefenition = value;
                timingEventController.ComponentDefenition = value;
                asyncEventController.ComponentDefenition = value;
                syncEventController.ComponentDefenition = value;
                oneTimeEventController.ComponentDefenition = value;
                RefreshGridViews();
            }
            get
            {
                return _componentDefenition;
            }
        }


        public void PerInstanceMemoryNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {            
            if (allowUpdater.IsUpdateAllowed)
            {
                int index = perInstanceGrid.SelectedIndex;
                if ((index < perInstanceGrid.Items.Count) && (index >= 0))
                {
                    String newName = (sender as TextBox).Text;
                    if (NameUtils.CheckComponentName(newName))
                    {
                        _componentDefenition.PerInstanceMemoryList[index].Name = newName;
                        AutosarApplication.GetInstance().SyncronizePimNames(_componentDefenition);
                        tree.UpdateAutosarTreeView(tree.SelectedItem);
                    }
                }
            }     
        }

        public void CDataNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (allowUpdater.IsUpdateAllowed)
            {
                int index = CDataGrid.SelectedIndex;
                if ((index < CDataGrid.Items.Count) && (index >= 0))
                {
                    String newName = (sender as TextBox).Text;
                    if (NameUtils.CheckComponentName(newName))
                    {
                        _componentDefenition.CDataDefenitions[index].Name = newName;
                        AutosarApplication.GetInstance().SyncronizeCDataNames(_componentDefenition);
                        tree.UpdateAutosarTreeView(tree.SelectedItem);
                    }
                }
            }
        }

        public void PerInstanceMemoryName_ChangeDatatype_Button_Click(object sender, RoutedEventArgs e)
        {
            int index = perInstanceGrid.SelectedIndex;
            if ((index < perInstanceGrid.Items.Count) && (index >= 0))
            {
                List<IGUID> datatypes = AutosarApplication.GetInstance().GetAllDataTypes();
                System.Windows.Forms.DialogResult result = DatatypeForm.GetInstance().ShowForm(datatypes, _componentDefenition.PerInstanceMemoryList[index].DatatypeGuid);
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    _componentDefenition.PerInstanceMemoryList[index].DatatypeGuid = DatatypeForm.GetInstance().SelectedDatatype.GUID;
                    AutosarApplication.GetInstance().SyncronizePerInstanceMemory(_componentDefenition);
                    UpdatePerInstanceMemoryGrid();
                }
            }
        }

        public void CData_ChangeDatatype_Button_Click(object sender, RoutedEventArgs e)
        {
            int index = CDataGrid.SelectedIndex;
            if ((index < CDataGrid.Items.Count) && (index >= 0))
            {
                List<IGUID> datatypes = AutosarApplication.GetInstance().GetAllDataTypes();
                System.Windows.Forms.DialogResult result = DatatypeForm.GetInstance().ShowForm(datatypes, _componentDefenition.CDataDefenitions[index].DatatypeGuid);
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    _componentDefenition.CDataDefenitions[index].DatatypeGuid = DatatypeForm.GetInstance().SelectedDatatype.GUID;
                    AutosarApplication.GetInstance().SyncronizeCData(_componentDefenition);
                    UpdateCDataGrid();
                }
            }
        }

        public void PerInstanceMemoryName_DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int index = perInstanceGrid.SelectedIndex;
            if ((index < perInstanceGrid.Items.Count) && (index >= 0))
            {
                PimDefenition piDef = perInstanceGrid.Items[index] as PimDefenition;
                bool delete = MessageUtilities.AskToDelete("Do you want to delete " + piDef.Name + "?");
                if (delete == true)
                {
                    _componentDefenition.PerInstanceMemoryList.Remove(piDef);
                    AutosarApplication.GetInstance().SyncronizePerInstanceMemory(_componentDefenition);
                    tree.UpdateAutosarTreeView(tree.SelectedItem);
                    UpdatePerInstanceMemoryGrid();
                }
            }
        }

        public void CData_DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int index = CDataGrid.SelectedIndex;
            if ((index < CDataGrid.Items.Count) && (index >= 0))
            {
                CDataDefenition cdataDef = CDataGrid.Items[index] as CDataDefenition;
                bool delete = MessageUtilities.AskToDelete("Do you want to delete " + cdataDef.Name + "?");
                if (delete == true)
                {
                    _componentDefenition.CDataDefenitions.Remove(cdataDef);
                    AutosarApplication.GetInstance().SyncronizeCData(_componentDefenition);
                    tree.UpdateAutosarTreeView(tree.SelectedItem);
                    UpdateCDataGrid();
                }
            }
        }


        public void RefreshGridViews()
        {
            allowUpdater.StopUpdate();

            componentDefenitionName_TextBox.Text = _componentDefenition.Name;
            multipleInstantiation_CheckBox.IsChecked = _componentDefenition.MultipleInstantiation;

            portsGrid.ItemsSource = null;
            portsGrid.ItemsSource = _componentDefenition.Ports;

            runnablesGrid.ItemsSource = null;
            runnablesGrid.ItemsSource = _componentDefenition.Runnables;

            CDataGrid.ItemsSource = null;
            CDataGrid.ItemsSource = _componentDefenition.CDataDefenitions;

            allowUpdater.AllowUpdate();

            UpdatePerInstanceMemoryGrid();
        }

        private void UpdatePerInstanceMemoryGrid()
        {
            allowUpdater.StopUpdate();
            perInstanceGrid.ItemsSource = null;
            perInstanceGrid.ItemsSource = _componentDefenition.PerInstanceMemoryList;
            allowUpdater.AllowUpdate();
        }

        private void UpdateCDataGrid()
        {
            allowUpdater.StopUpdate();
            CDataGrid.ItemsSource = null;
            CDataGrid.ItemsSource = _componentDefenition.CDataDefenitions;
            allowUpdater.AllowUpdate();
        }

        public void ChangePortInterfaceButtonClick()
        {
            int index = portsGrid.SelectedIndex;
            if ((index < portsGrid.Items.Count) && (index >= 0))
            {
                List<IGUID> interfaces = null;
                if ((_componentDefenition.Ports[index].PortType == PortType.Client) || (_componentDefenition.Ports[index].PortType == PortType.Server))  
                {
                    interfaces = AutosarApplication.GetInstance().GetAllClientServerInterfacesNames();
                }
                else if ((_componentDefenition.Ports[index].PortType == PortType.Sender) || (_componentDefenition.Ports[index].PortType == PortType.Receiver))
                {
                    interfaces = AutosarApplication.GetInstance().GetAllSenderReceiverInterfacesNames();
                }

                System.Windows.Forms.DialogResult result = DatatypeForm.GetInstance().ShowForm(interfaces, _componentDefenition.Ports[index].InterfaceGUID);
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    _componentDefenition.Ports[index].InterfaceGUID = DatatypeForm.GetInstance().SelectedDatatype.GUID;
                    RefreshGridViews();
                }
            }
        }
        

        public bool DeletePortButtonClick()
        {
            int index = portsGrid.SelectedIndex;
            bool result = false;
            if ((index < portsGrid.Items.Count) && (index >= 0))
            {
                PortDefenition portDef = portsGrid.Items[index] as PortDefenition;
                bool delete = MessageUtilities.AskToDelete("Do you want to delete " + portDef.Name + "?");
                if (delete == true)
                {
                    _componentDefenition.Ports.Remove(portDef);
                    AutosarApplication.GetInstance().UpdatePortsInComponentInstances();
                    tree.UpdateAutosarTreeView(tree.SelectedItem);
                    RefreshGridViews();
                    result = true;
                }             
            }
            return result;
        }

        public bool DeleteRunnableButtonClick()
        {
            int index = runnablesGrid.SelectedIndex;
            bool result = false;
            if ((index < runnablesGrid.Items.Count) && (index >= 0))
            {
                bool delete = MessageUtilities.AskToDelete("Do you want to delete " + (runnablesGrid.Items[index] as RunnableDefenition).Name + "?");
                if (delete == true)
                {
                    _componentDefenition.Runnables.RemoveAt(index);
                    AutosarApplication.GetInstance().SyncronizeEvents(_componentDefenition);
                    tree.UpdateAutosarTreeView(tree.SelectedItem);
                    RefreshGridViews();
                    result = true;
                }
            }
            return result;
        }

        public void RenamePortTextEdit(string newName)
        {
            if (allowUpdater.IsUpdateAllowed)
            {
                int index = portsGrid.SelectedIndex;
                if ((index < portsGrid.Items.Count) && (index >= 0))
                {
                    if (NameUtils.CheckComponentName(newName))
                    {
                        _componentDefenition.Ports[index].Name = newName;
                        tree.UpdateAutosarTreeView(tree.SelectedItem);
                    }
                }
            }
        }

        public void AddRunnableButton_Click()
        {
            RunnableDefenition runnable = ComponentFabric.GetInstance().CreateRunnableDefenition("Refresh");
            _componentDefenition.Runnables.Add(runnable);
            _componentDefenition.Runnables.DoSort();
            AutosarApplication.GetInstance().SyncronizeEvents(_componentDefenition);
            RefreshGridViews();
            tree.UpdateAutosarTreeView(tree.SelectedItem);
        }

        public void RenameRunnable_TextEdit(string newName)
        {
            if (allowUpdater.IsUpdateAllowed)
            {
                int index = runnablesGrid.SelectedIndex;
                if ((index < runnablesGrid.Items.Count) && (index >= 0))
                {
                    if (NameUtils.CheckComponentName(newName))
                    {
                        _componentDefenition.Runnables[index].Name = newName;
                        AutosarApplication.GetInstance().SyncronizeEvents(_componentDefenition);
                        tree.UpdateAutosarTreeView(tree.SelectedItem);
                    }
                }
            }
        }

        public bool AddPortButtonClick()
        {
            if (ComponentDefenition != null)
            {
                if (AddPortForm.GetInstance().ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ComponentDefenition.Ports.Add(AddPortForm.GetInstance().CreatedPortDefenition);
                    ComponentDefenition.Ports.DoSort();
                    AutosarApplication.GetInstance().UpdatePortsInComponentInstances();
                    RefreshGridViews();
                    tree.UpdateAutosarTreeView(tree.SelectedItem);
                    return true;
                }
            }
            return false;
        }
    }
}
