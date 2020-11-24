using AutosarGuiEditor.Source.Forms.Controls;
using AutosarGuiEditor.Source.Painters;
using AutosarGuiEditor.Source.Painters.Components.CData;
using AutosarGuiEditor.Source.Painters.Components.PerInstance;
using AutosarGuiEditor.Source.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AutosarGuiEditor.Source.Controllers
{
    public class ComponentPropertiesController
    {
        public ComponentPropertiesController(TextBox ComponentNameTextBox, TextBox ComponentDefenitionTextBox, DataGrid PerInstanceGrid, DataGrid CDataGrid, AutosarTreeViewControl treeView)
        {
            componentNameTextBox = ComponentNameTextBox;
            componentNameTextBox.TextChanged += componentNameTextBox_TextChanged;
            componentDefenitionTextBox = ComponentDefenitionTextBox;
            this.treeView = treeView;
            perInstanceGrid = PerInstanceGrid;
            this.CDataGrid = CDataGrid;
        }

        void componentNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (allowUpdater.IsUpdateAllowed)
            {
                if (NameUtils.CheckComponentName(componentNameTextBox.Text))
                {
                    component.Name = componentNameTextBox.Text;
                    AutosarApplication.GetInstance().UpdateNamesOfConnectionLines();
                    treeView.UpdateAutosarTreeView();
                }
            }
        }

        ComponentInstance component;
        public ComponentInstance Component
        {
            set 
            {
                component = value;
                allowUpdater.StopUpdate();
                UpdateControls();
                allowUpdater.AllowUpdate();
            }
            get
            {
                return component;
            }
        }

        
        TextBox componentNameTextBox;
        TextBox componentDefenitionTextBox;
        DataGrid perInstanceGrid;
        DataGrid CDataGrid;
        AutosarTreeViewControl treeView;

        AllowUpdater allowUpdater = new AllowUpdater();
        PimDefaultValuesList currentPimDefaultList;
        CDataDefaultValuesList currentCDataDefalutList;

        void UpdateControls()
        {
            if (component != null)
            {
                componentNameTextBox.Text = component.Name;
                componentDefenitionTextBox.Text = component.ComponentDefenition.Name;

                /* Pims */
                currentPimDefaultList = component.PerInstanceMemories.CollectAllPimValuesForGrid();
                
                perInstanceGrid.ItemsSource = null;
                perInstanceGrid.ItemsSource = currentPimDefaultList;

                /* CData */
                currentCDataDefalutList = component.CDataInstances.CollectAllCDataValuesForGrid();

                CDataGrid.ItemsSource = null;
                CDataGrid.ItemsSource = currentCDataDefalutList;
            }
        }

        public void PimDefaultValue_ValueColomn_TextEdit(object sender, TextChangedEventArgs e)
        {
            if (allowUpdater.IsUpdateAllowed)
            {
                if (currentPimDefaultList != null)
                {
                    int index = perInstanceGrid.SelectedIndex;
                    if ((index < perInstanceGrid.Items.Count) && (index >= 0) && (index < currentPimDefaultList.Count))
                    {
                        String newDefaultValue = (sender as TextBox).Text;
                        currentPimDefaultList[index].DefaultValue = newDefaultValue;
                    }
                }
            }     
        }

        public void CDataDefaultValue_ValueColomn_TextEdit(object sender, TextChangedEventArgs e)
        {
            if (allowUpdater.IsUpdateAllowed)
            {
                if (currentCDataDefalutList != null)
                {
                    int index = CDataGrid.SelectedIndex;
                    if ((index < CDataGrid.Items.Count) && (index >= 0) && (index < currentCDataDefalutList.Count))
                    {
                        String newDefaultValue = (sender as TextBox).Text;
                        currentCDataDefalutList[index].DefaultValue = newDefaultValue;
                    }
                }
            }
        }
    }
}
