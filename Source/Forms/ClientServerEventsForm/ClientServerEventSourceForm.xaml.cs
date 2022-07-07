using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.PortDefenitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AutosarGuiEditor.Source.Forms.Controls;

namespace AutosarGuiEditor.Source.Forms.ClientServerEventsForm
{
    /// <summary>
    /// Interaction logic for ClientServerEventSourceForm.xaml
    /// </summary>
    public partial class ClientServerEventSourceForm : Window
    {
        ApplicationSwComponentType compDef;

        public ClientServerEventSourceForm(ApplicationSwComponentType compDef, Boolean isAsync)
        {
            this.compDef = compDef;
            
            InitializeComponent();

            /* Create tree from all client-server ports with specified asyncronous behaviour */
            
            foreach(PortDefenition portDef in compDef.Ports)
            {
                if (portDef.PortType == PortType.Server)
                {
                    ClientServerInterface csInterface = portDef.InterfaceDatatype as ClientServerInterface;
                    if (csInterface.IsAsync == isAsync)
                    {
                        TreeViewItem portTreeItem = new TreeViewItem();
                        portTreeItem.Header = portDef.Name;
                        portTreeItem.Tag = portDef;
                        portTreeItem.IsExpanded = true;

                        foreach (ClientServerOperation op in csInterface.Operations)
                        {
                            TreeViewItem operationTreeItem = new TreeViewItem();
                            operationTreeItem.Header = op.Name;
                            operationTreeItem.Tag = op;
                            portTreeItem.Items.Add(operationTreeItem);                            
                        }

                        treeView.Items.Add(portTreeItem);
                    }
                }
            }
        }
        
        ClientServerOperation operation;
        public ClientServerOperation Operation
        {
            get
            {
                return operation;
            }
        }

        PortDefenition port;
        public PortDefenition Port
        {
            get
            {
                return port;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((treeView.SelectedItem != null) && (treeView.SelectedItem is TreeViewItem))
            {
                operation = (treeView.SelectedItem as TreeViewItem).Tag as ClientServerOperation;
                port      = ((treeView.SelectedItem as TreeViewItem).Parent as TreeViewItem).Tag as PortDefenition;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Operation is not selected");
            }
        }
    }
}
