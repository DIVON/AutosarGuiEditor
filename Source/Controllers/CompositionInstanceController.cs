using AutosarGuiEditor.Source.Composition;
using AutosarGuiEditor.Source.Forms;
using AutosarGuiEditor.Source.Forms.Controls;
using AutosarGuiEditor.Source.Painters.PortsPainters;
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
    public class CompositionInstanceController
    {
        AutosarTreeViewControl tree;
        TextBox compositoinNameTextBox;
        DataGrid portsGrid;
        TabItem compositionTab;

        public AllowUpdater allowUpdater = new AllowUpdater();

        public CompositionInstanceController(AutosarTreeViewControl AutosarTree, TextBox CompositoinNameTextBox, DataGrid PortsGrid, Button AddPort_Button, TabItem compositionTab)
        {
            tree = AutosarTree;
            this.portsGrid = PortsGrid;
            this.compositionTab = compositionTab;
            compositoinNameTextBox = CompositoinNameTextBox;
            compositoinNameTextBox.TextChanged += compositionNameTextBox_TextChanged;
            AddPort_Button.Click += AddPortButtonClick;
        }

        void compositionNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (allowUpdater.IsUpdateAllowed)
            {
                /* Do not change the name of main composition! */
                if (composition.Equals(AutosarApplication.GetInstance().Compositions.GetMainComposition()))
                {
                    return;
                }

                String newName = (sender as TextBox).Text;
                if (NameUtils.CheckComponentName(newName))
                {
                    composition.Name = newName;
                    
                    compositionTab.Header = "Composition: " + composition.Name;

                    TreeViewItem item = tree.GetItem(composition);
                    if (item != null)
                    {
                        item.Header = composition.Name;
                    }

                    AutosarApplication.GetInstance().UpdateNamesOfConnectionLines();
                    tree.UpdateAutosarTreeView();
                }
            }   
        }

        CompositionInstance composition;
        public CompositionInstance Composition
        {
            set
            {
                composition = value;
                allowUpdater.StopUpdate();
                UpdateControls();
                allowUpdater.AllowUpdate();
            }
            get
            {
                return composition;
            }
        }

        String correctUnderscores(String str)
        {
            /* This function is needed because single underscore is removed from header. */
            return str.Replace("_", "__");
        }

        public void UpdateControls()
        {
            compositoinNameTextBox.Text = composition.Name;
            compositionTab.Header = correctUnderscores(composition.Name);
            AutosarApplication.GetInstance().ActiveComposition = composition;
            UpdatePortsGrid();
        }

        public void UpdatePortsGrid()
        {
            portsGrid.ItemsSource = null;
            portsGrid.ItemsSource = composition.PortsDefenitions;
        }

        public void ChangePortInterfaceButtonClick()
        {
            int index = portsGrid.SelectedIndex;
            if ((index < portsGrid.Items.Count) && (index >= 0))
            {
                List<IGUID> interfaces = null;
                if ((composition.PortsDefenitions[index].PortType == PortType.Client) || (composition.PortsDefenitions[index].PortType == PortType.Server))
                {
                    interfaces = AutosarApplication.GetInstance().GetAllClientServerInterfacesNames();
                }
                else if ((composition.PortsDefenitions[index].PortType == PortType.Sender) || (composition.PortsDefenitions[index].PortType == PortType.Receiver))
                {
                    interfaces = AutosarApplication.GetInstance().GetAllSenderReceiverInterfacesNames();
                }

                System.Windows.Forms.DialogResult result = DatatypeForm.GetInstance().ShowForm(interfaces, composition.PortsDefenitions[index].InterfaceGUID);
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    composition.PortsDefenitions[index].InterfaceGUID = DatatypeForm.GetInstance().SelectedDatatype.GUID;
                    UpdatePortsGrid();
                }
            }
        }

        public void CreateNewComposition()
        {
            CompositionInstance newComposition = new CompositionInstance();
            newComposition.Painter.Width = 100;
            newComposition.Painter.Height = 100;
            newComposition.UpdateAnchorsPositions();
            newComposition.Name = FindNewNameForComposition();
            AutosarApplication.GetInstance().Compositions.Add(newComposition);
            tree.UpdateAutosarTreeView(newComposition);
        }

        String FindNewNameForComposition()
        {
            const String BaseCompositionName = "Composition";
            int index = 0;
            while (AutosarApplication.GetInstance().Compositions.FindObject(BaseCompositionName + index.ToString()) != null)
            {
                index++;
            }

            return BaseCompositionName;
        }

        public void PortNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (allowUpdater.IsUpdateAllowed)
            {
                int index = portsGrid.SelectedIndex;
                if ((index < portsGrid.Items.Count) && (index >= 0))
                {
                    String newName = (sender as TextBox).Text;
                    if (NameUtils.CheckComponentName(newName))
                    {
                        composition.PortsDefenitions[index].Name = newName;
                        composition.InternalPortsInstances[index].Name = newName;
                        composition.Ports[index].Name = newName;                        
                        tree.UpdateAutosarTreeView(tree.SelectedItem);
                    }
                }
            }
        }

        public void Port_DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int index = portsGrid.SelectedIndex;
            if ((index < portsGrid.Items.Count) && (index >= 0))
            {
                PortDefenition portDef = portsGrid.Items[index] as PortDefenition;
                bool delete = MessageUtilities.AskToDelete("Do you want to delete " + portDef.Name + "?");
                if (delete == true)
                {
                    AutosarApplication.GetInstance().DeletePort(composition.GetInternalPortPainter(portDef));
                    AutosarApplication.GetInstance().DeletePort(composition.GetExternalPortPainter(portDef));

                    PortPainter internalPort = composition.GetInternalPortPainter(portDef);
                    if (internalPort != null)
                    {
                        composition.InternalPortsInstances.Remove(internalPort);
                    }

                    PortPainter externalPort = composition.GetExternalPortPainter(portDef);
                    if (externalPort != null)
                    {
                        composition.Ports.Remove(externalPort);
                    }

                    composition.PortsDefenitions.Remove(portDef);
                    tree.UpdateAutosarTreeView(tree.SelectedItem);
                    UpdatePortsGrid();
                }
            }
        }

        public void AddPortButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (composition != null)
            {
                if (AddPortForm.GetInstance().ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    PortDefenition newPortDef = AddPortForm.GetInstance().CreatedPortDefenition;
                    if (newPortDef != null)
                    {
                        composition.AddPort(newPortDef);

                        UpdatePortsGrid();
                        tree.UpdateAutosarTreeView(tree.SelectedItem);
                    }
                }
            }
        }
    }
}
