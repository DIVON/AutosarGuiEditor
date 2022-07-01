using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;
using AutosarGuiEditor.Source.Forms;
using AutosarGuiEditor.Source.Forms.Controls;
using AutosarGuiEditor.Source.SystemInterfaces;
using AutosarGuiEditor.Source.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AutosarGuiEditor.Source.Controllers
{
    public class ClientServerInterfaceController
    {
        AllowUpdater allowUpdater = new AllowUpdater();
        AutosarTreeViewControl tree;
        DataGrid grid;
        TextBox nameTextBox;
        CheckBox asyncCheckBox;

        List<ClientServerInterfaceGridViewField> gridElements = new List<ClientServerInterfaceGridViewField>();

        public ClientServerInterfaceController(AutosarTreeViewControl AutosarTree, DataGrid grid, TextBox nameTextBox, CheckBox asyncCheckBox)
        {
            this.tree = AutosarTree;
            this.grid = grid;
            this.nameTextBox = nameTextBox;
            this.nameTextBox.TextChanged += nameTextBox_TextChanged;
            
            this.asyncCheckBox = asyncCheckBox;
            this.asyncCheckBox.Checked += asyncCheckBox_Checked;
            this.asyncCheckBox.Unchecked += asyncCheckBox_Checked;
        }

        void asyncCheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (allowUpdater.IsUpdateAllowed)
            {
                _csInterface.IsAsync = asyncCheckBox.IsChecked.Value;
            }
        }

        void nameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (allowUpdater.IsUpdateAllowed)
            {

                String newName = (sender as TextBox).Text;
                if (NameUtils.CheckComponentName(newName))
                {
                    csInterface.Name = newName;

                    TreeViewItem item = tree.GetItem(csInterface);
                    if (item != null)
                    {
                        item.Header = csInterface.Name;
                    }
                }
            }   
        }

        
        public ClientServerInterface CreateClientServerInterface()
        {
            string templateName = "iClientServer";
            if (AutosarApplication.GetInstance().ClientServerInterfaces.FindObject(templateName) != null)
            {
                int index = 0;
                while (AutosarApplication.GetInstance().ClientServerInterfaces.FindObject(templateName) != null)
                {
                    index++;
                    templateName = "iClientServer" + index.ToString();
                }
            }

            ClientServerInterface csInterface = DataTypeFabric.Instance().CreateClientServerInterface(templateName);
            AutosarApplication.GetInstance().ClientServerInterfaces.Add(csInterface);
            return csInterface;
        }

        ClientServerInterface _csInterface;
        public ClientServerInterface csInterface
        {
            set
            {
                allowUpdater.StopUpdate();
                _csInterface = value;

                nameTextBox.Text = _csInterface.Name;
                asyncCheckBox.IsChecked = _csInterface.IsAsync;

                RefreshGridView();

                allowUpdater.AllowUpdate();
            }
            get
            {
                return _csInterface;
            }
        }
        
        private ClientServerOperation GetCurrentCsOperation()
        {
            if (csInterface.Operations.Count == 1)
            {
                return csInterface.Operations[0];
            }
            if (grid.SelectedIndex < 0)
            {
                return null;
            }
            for (int i = grid.SelectedIndex; i >= 0; i--)
            {
                if (gridElements[i].containObject is ClientServerOperation)
                {
                    return (gridElements[i].containObject as ClientServerOperation);
                }
            }
            
            return null;
        }

        public void AddField()
        {
            ClientServerOperation currOperation = GetCurrentCsOperation();
            if (currOperation != null)
            {
                currOperation.Fields.Add(new ClientServerOperationField());
                tree.UpdateAutosarTreeView();
                RefreshGridView();
            }
        }

        public void AddOperation()
        {
            ClientServerOperation newOperation = new ClientServerOperation();
            csInterface.Operations.Add(newOperation);
            tree.UpdateAutosarTreeView();
            RefreshGridView();
        }

        public void RefreshGridView()
        {
            gridElements.Clear();
            
            /* Fill grid with elements */
            foreach(ClientServerOperation operation in csInterface.Operations)
            {
                gridElements.Add(new ClientServerInterfaceGridViewField(operation));
                foreach (ClientServerOperationField operationField in operation.Fields)
                {
                    gridElements.Add(new ClientServerInterfaceGridViewField(operationField));
                }
            }

            grid.ItemsSource = null;
            grid.ItemsSource = gridElements;
        }

        public void ChangeDatatypeButtonClick()
        {
            int index = grid.SelectedIndex;

            if (gridElements[index].containObject is ClientServerOperationField)
            {
                ClientServerOperationField field = (gridElements[index].containObject as ClientServerOperationField);
                List<IGUID> datatypes = AutosarApplication.GetInstance().GetAllDataTypes();
                System.Windows.Forms.DialogResult result = DatatypeForm.GetInstance().ShowForm(datatypes, field.BaseDataTypeGUID);
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    field.BaseDataTypeGUID = DatatypeForm.GetInstance().SelectedDatatype.GUID;
                    RefreshGridView();
                }
            }
        }

        public void SelectDirectionClick(String direction)
        {
            int index = grid.SelectedIndex;
            if ((index < grid.Items.Count) && (index >= 0))
            {
                gridElements[index].Direction = direction;
                RefreshGridView();
            }
        }

        public void DeleteButtonClick()
        {
            int index = grid.SelectedIndex;
            if ((index < grid.Items.Count) && (index >= 0))
            {
                if (gridElements[index].containObject is ClientServerOperation)
                {
                    csInterface.Operations.Remove((gridElements[index].containObject as ClientServerOperation));
                    RefreshGridView();
                    tree.UpdateAutosarTreeView();
                }
                else if (gridElements[index].containObject is ClientServerOperationField)
                {
                    ClientServerOperation operation = GetCurrentCsOperation();
                    if (operation != null)
                    {
                        operation.Fields.Remove(gridElements[index].containObject as ClientServerOperationField);
                        RefreshGridView();
                        tree.UpdateAutosarTreeView();
                    }
                }
            }
        }

        public void RenameFieldTextEdit(TextBox textbox)
        {
            int index = grid.SelectedIndex;
            if ((index < grid.Items.Count) && (index >= 0))
            {
                if (NameUtils.CheckComponentName(textbox.Text))
                {
                    gridElements[index].Name = textbox.Text;
                    tree.UpdateAutosarTreeView();
                }
            }
        }
    }
}
