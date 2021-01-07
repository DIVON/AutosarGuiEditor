using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Composition;
using AutosarGuiEditor.Source.Forms.Controls;
using AutosarGuiEditor.Source.Painters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AutosarGuiEditor.Source.Controllers
{
    public class AddObjectController
    {
        public AutosarTreeViewControl treeView;

        public AddObjectController(AutosarTreeViewControl treeView)
        {
            this.treeView = treeView;
        }

        public Boolean IsPressed = false;
        
        private ComponentDefenition compDef;

        public void MouseDownOnTreeViewMenu()
        {
            if (treeView.SelectedItem is TreeViewItem)
            {
                TreeViewItem selectedItem = treeView.SelectedItem as TreeViewItem;
                if (selectedItem.Tag is ComponentDefenition)
                {
                    compDef = (selectedItem.Tag as ComponentDefenition);
                    DataObject dataObj = new DataObject(compDef);
                    DragDrop.DoDragDrop(selectedItem, dataObj, DragDropEffects.Copy);
                }
            }
        }

        public void Clear()
        {
            IsPressed = false;
            compDef = null;
        }

        public void ViewPortImage_Drop(DragEventArgs e, double worldCoordX, double worldCoordY)
        {
            object obj = e.Data.GetData(typeof(ComponentDefenition));
            if (obj is ComponentDefenition)
            {
                ComponentDefenition realDefenition = (ComponentDefenition)obj;
                int count = AutosarApplication.GetInstance().GetComponentDefenitionCount(realDefenition);
                if (realDefenition.MultipleInstantiation || ((count == 0) && (!realDefenition.MultipleInstantiation)))
                {
                    ComponentInstance compInstance = ComponentFabric.GetInstance().CreateComponent(realDefenition, worldCoordX, worldCoordY);
                    compInstance.UpdateAnchorsPositions();
                    CompositionInstance activeComposition = AutosarApplication.GetInstance().ActiveComposition;
                    activeComposition.ComponentInstances.Add(compInstance);
                }
            }
        }
    }
}
