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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.DataTypes.Enum;
using AutosarGuiEditor.Source.DataTypes.ComplexDataType;
using AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver;
using AutosarGuiEditor.Source.Painters;
using AutosarGuiEditor.Source.Painters.PortsPainters;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Interfaces;
using AutosarGuiEditor.Source.SystemInterfaces;
using System.Reflection;
using System.ComponentModel;
using AutosarGuiEditor.Source.Composition;

namespace AutosarGuiEditor.Source.Forms.Controls
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:AutosarGuiEditor.Source.Forms.Controls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:AutosarGuiEditor.Source.Forms.Controls;assembly=AutosarGuiEditor.Source.Forms.Controls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:AutosarTreeViewControl/>
    ///
    /// </summary>
    /// 
    public class AutosarTreeViewControl : TreeView
    {
        AutosarApplication autosarApp;
        public AutosarApplication Autosar {
            set
            {
                autosarApp = value;
                UpdateAutosarTreeView(null);
            }
            get
            {
                return autosarApp;
            }
        }

        public AutosarTreeViewControl()
            : base()
        {
            /* Create base folders */
            this.SelectedItemChanged += AutosarTreeViewControl_SelectedItemChanged;
            this.KeyDown += AutosarTreeViewControl_KeyDown;
        }

        void AutosarTreeViewControl_KeyDown(object sender, KeyEventArgs e)
        {
            
        }

        

        void AutosarTreeViewControl_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem)
            {
                TreeViewItem item = (e.NewValue as TreeViewItem);
                if (item.Tag is ComponentInstance)
                {
                    autosarApp.UnselectComponents();
                    (item.Tag as ComponentInstance).Select();
                }
                if (item.Tag is PortPainter)
                {
                    autosarApp.UnselectComponents();
                    (item.Tag as PortPainter).Selected = true;
                }
                if (item.Tag is PortConnection)
                {
                    autosarApp.UnselectComponents();
                    (item.Tag as PortConnection).Select();
                }
            }
        }

        public TreeViewItem GetItem(Object tag)
        {
            TreeViewItem returnedObj = null;
            foreach(TreeViewItem rootItem in this.Items)
            {
                returnedObj = ObjectFound(rootItem, tag);
                if (returnedObj != null)
                {
                    return returnedObj;
                }
            }

            return returnedObj;
        }

        public void UpdateAutosarTreeView()
        {
            /* Find all properties in Autosar Application */
            /* Find all IGuidObjects */
            String res = "";
            PropertyInfo[] allProperties = AutosarApplication.GetInstance().GetType().GetProperties();
            List<PropertyInfo> properties = new List<PropertyInfo>();
            foreach(PropertyInfo property in allProperties)
            {
                if (property.GetValue(AutosarApplication.GetInstance()) as IAutosarTreeList != null)
                {
                    properties.Add(property);
                }
            }

            properties.Sort(delegate(PropertyInfo x, PropertyInfo y)
            {
                IAutosarTreeList list1 = x.GetValue(AutosarApplication.GetInstance()) as IAutosarTreeList;
                IAutosarTreeList list2 = y.GetValue(AutosarApplication.GetInstance()) as IAutosarTreeList;
                String name1 = list1.GetName();
                String name2 = list2.GetName();
                if ((name1 != null) && (name2 != null))
                {
                    string xName = name1.ToString();
                    string yName = name2.ToString();

                    if (xName == null && yName == null) return 0;
                    else if (xName == null) return -1;
                    else if (yName == null) return 1;
                    else return xName.CompareTo(yName);
                }

                throw new Exception("Sort exception! Properties not exists!");
            });

            /* Delete unexist */
            //DeleteUnexists(properties);
            

            /* Add new */
            TreeViewItem root = new TreeViewItem();
            foreach (PropertyInfo property in properties)
            {
                res += property.Name + " " +Environment.NewLine;
                IAutosarTreeList treeList = property.GetValue(AutosarApplication.GetInstance()) as IAutosarTreeList;
                if (treeList != null)
                {
                    /* Create root for the list */
                    CreateTreeForList(root, treeList);
                }
            }

            /* Sort */
            for (int i = 0; i < root.Items.Count; i++)
            {
                SortChilds(root.Items[i] as TreeViewItem);
            }

            /* syncronize root and items */
            SyncronizeRoot(root.Items.Count);
            for (int i = 0; i < root.Items.Count; i++)
            {
                Syncronize(root.Items[i] as TreeViewItem, Items[i] as TreeViewItem);
            }

            /* Sort */
            for (int i = 0; i < root.Items.Count; i++)
            {
                SortChilds(root.Items[i] as TreeViewItem);
            }
        }

        public void SortChilds(TreeViewItem item)
        {            
            foreach(TreeViewItem childItem in item.Items)
            {                
                childItem.Items.IsLiveSorting = true;                
                childItem.Items.SortDescriptions.Add(new SortDescription("Header", ListSortDirection.Ascending));
                childItem.Items.LiveSortingProperties.Add("Header");
            }                        
        }

        private void SyncronizeRoot( int rootItemsCount)
        {
            /* syncronize root and items */
            if (Items.Count < rootItemsCount)
            {
                int itemsToAdd = rootItemsCount - Items.Count;
                for (int i = 0; i < itemsToAdd; i++)
                {
                    TreeViewItem item = new TreeViewItem();
                    Items.Add(item);
                }
            }
            else if (Items.Count > rootItemsCount)
            {
                while (Items.Count != rootItemsCount)
                {
                    Items.RemoveAt(Items.Count - 1);
                }
            }
        }


        TreeViewItem findChild(TreeViewItem itemForFind, ItemCollection items)
        {
            /* Find the same in the new root*/
            if ((itemForFind.Tag is IGUID) || (itemForFind.Tag is IAutosarTreeList))
            {
                for (int j = items.Count - 1; j >= 0; j--)
                {
                    TreeViewItem item = (items[j] as TreeViewItem);
                    if (item != null)
                    {
                        if (item.Tag.Equals(itemForFind.Tag))
                        {
                            return item;                            
                        }
                    }
                }
            }
            else /* empty */
            {
                for (int j = items.Count - 1; j >= 0; j--)
                {
                    TreeViewItem item = (items[j] as TreeViewItem);
                    if ((item!= null) && (item.Tag == null))
                    {
                        if (item.Header.Equals(itemForFind.Header))
                        {
                            return item;  
                        }
                    }
                }
            }
            return null;
        }

        private void SyncronizeCount(TreeViewItem rootItem, TreeViewItem nextItem)
        {
            /* Delete unexists */
            for (int i = nextItem.Items.Count - 1; i >= 0; i--)
            {                
                TreeViewItem item = findChild((nextItem.Items[i] as TreeViewItem), rootItem.Items);

                if (item == null)
                {
                    nextItem.Items.RemoveAt(i);
                }
                
            }

            /* Add new and synchronize */
            foreach (TreeViewItem rootItemIterator in rootItem.Items)
            {
                TreeViewItem item = findChild(rootItemIterator, nextItem.Items);
                
                /* If item not found int AutosarTree */
                if (item == null)
                {
                    /* Add it to the list */
                    /* Make a copy */
                    TreeViewItem newItem = new TreeViewItem();
                    newItem.Tag = rootItemIterator.Tag;
                    newItem.Header = rootItemIterator.Header;
                    nextItem.Items.Add(newItem);
                }
                else
                {
                    item.Header = (rootItemIterator).Header;
                }
            }
        }

        private void Syncronize(TreeViewItem rootItem, TreeViewItem nextItem)
        {            
            if (!rootItem.Header.Equals(nextItem.Header))
            {
                nextItem.Header = rootItem.Header;
            }
            if (rootItem.Tag != nextItem.Tag)
            {
                nextItem.Tag = rootItem.Tag;
            }
            
            /* Find child with tag */
            SyncronizeCount(rootItem, nextItem);
            

            for(int i = 0; i <rootItem.Items.Count; i++)
            {
                /* here searching shall exist */
                TreeViewItem item = findChild(rootItem.Items[i] as TreeViewItem, nextItem.Items);
                Syncronize(rootItem.Items[i] as TreeViewItem, item);
            }
        }

        TreeViewItem CreateTreeViewItem(TreeViewItem root, String header)
        {
            TreeViewItem newTreeItem = new TreeViewItem();
            newTreeItem.Header = header;
            newTreeItem.FontWeight = FontWeights.Normal;
            newTreeItem.Tag = null;
            root.Items.Add(newTreeItem);
            return newTreeItem;
        }

        TreeViewItem CreateItemsForGuid(TreeViewItem root, IGUID guid)
        {
            TreeViewItem newTreeItem = new TreeViewItem();
            newTreeItem.Header = guid.Name;
            newTreeItem.FontWeight = FontWeights.Normal;
            newTreeItem.Tag = guid;
            root.Items.Add(newTreeItem);
            return newTreeItem;
        }

        TreeViewItem CreateItemsForList(TreeViewItem root, IAutosarTreeList list)
        {
            TreeViewItem newTreeItem = new TreeViewItem();
            newTreeItem.Header = list.GetName();
            newTreeItem.FontWeight = FontWeights.Normal;
            newTreeItem.Tag = list;
            root.Items.Add(newTreeItem);
            return newTreeItem;
        }

        void CreateTreeForList(TreeViewItem root, IAutosarTreeList list)
        {
            /* */
            bool needAdd = false;
            TreeViewItem newTreeItem;
            if (list.CreateRoot() || (root == null))
            {
                               
                newTreeItem = new TreeViewItem();
                newTreeItem.Header = list.GetName();
                newTreeItem.FontWeight = FontWeights.Normal;
                newTreeItem.Tag = list;
                needAdd = true;
            }
            else
            {
                needAdd = false;
                newTreeItem = root;
            }
                                

            /* Create items for all properties */
            List<IGUID> guids = list.GetItems();
            if (guids != null)
            {
                foreach (IGUID guid in guids)
                {
                    TreeViewItem item = CreateItemsForGuid(newTreeItem, guid);

                    /* Create leafes for the lists*/
                    List<IAutosarTreeList> lists = guid.GetLists();      
                    if (lists != null)
                    {
                        if (lists.Count > 0)
                        {
                            
                            foreach (IAutosarTreeList treeList in lists)
                            {
                                CreateTreeForList(item, treeList);
                            }
                        }
                    }
                }
            }

            if (needAdd)
            {
                if (root != null)
                {
                    root.Items.Add(newTreeItem);
                }
                else
                {
                    newTreeItem.FontWeight = FontWeights.Bold;
                    this.Items.Add(newTreeItem);
                }
            }
        }

        public void UpdateAutosarTreeView(object selectedItem)
        {
            object selectedObject = null;
            if (selectedItem == null)
            {
                if (SelectedItem != null)
                {
                    selectedObject = (this.SelectedItem as TreeViewItem).Tag;
                }
            }
            else 
            {
                selectedObject = selectedItem;
            }            

            UpdateAutosarTreeView();
            if (selectedItem != null)
            {
                SelectTreeViewItem(this.Items, selectedObject);
            }
            
        }

        

        private TreeViewItem SelectTreeViewItem(ItemCollection Collection, Object obj)
        {
            if (Collection == null) return null;
            foreach (TreeViewItem Item in Collection)
            {
                /// Find in current
                if (Item.Tag== obj)
                {
                    Item.IsSelected = true;
                    return Item;
                }
                /// Find in Childs
                if (Item.Items.Count > 0)
                {
                    TreeViewItem childItem = this.SelectTreeViewItem(Item.Items, obj);
                    if (childItem != null)
                    {
                        Item.IsExpanded = true;
                        return childItem;
                    }
                }
            }
            return null;
        }

        private TreeViewItem ObjectFound(TreeViewItem root, Object objectToFind)
        {
            foreach (TreeViewItem item in root.Items)
            {
                if (item.Tag == objectToFind)
                {
                    return item;
                }
            }
            return null;
        }


 

        public void CleanTree()
        {
            Items.Clear();
        }

        public TreeViewItem FindItem(TreeViewItem root, Object item)
        {
            foreach (TreeViewItem treeViewItem in root.Items)
            {
                if (treeViewItem.Tag == item)
                {
                    return treeViewItem;
                }
                else
                {
                    return FindItem(treeViewItem, item);
                }
            }
            return null;
        }

        //public TreeViewItem FindItem(TreeViewItem root, IGUID guidObject)
        //{
        //    if (root == null)
        //    {
        //        foreach (TreeViewItem treeViewItem in Items)
        //        {
        //            /* Item is not found. Try to find in their children */
        //            return FindItem(treeViewItem, guidObject);                    
        //        }
        //    }
        //    else
        //    {
        //        IGUID treeGuidObj = root.Tag as IGUID;
        //        if (treeGuidObj != null)
        //        {
        //            if (treeGuidObj.GUID.Equals(guidObject.GUID))
        //            {
        //                return root;
        //            }
        //        }

        //        foreach (TreeViewItem treeViewItem in root.Items)
        //        {
        //            treeGuidObj = treeViewItem.Tag as IGUID;
        //            if (treeGuidObj != null)
        //            {
        //                if (treeGuidObj.GUID.Equals(guidObject.GUID))
        //                {
        //                    return treeViewItem;
        //                }
        //            }

        //            /* Item is not found. Try to find in their children */
        //            return FindItem(treeViewItem, guidObject);
        //        }
        //    }
        //    return null;
        //}

        public bool DoesGuidExistInProperties(List<PropertyInfo> properties, IGUID guidObject)
        {
            foreach (PropertyInfo property in properties)
            {
                IAutosarTreeList treeList = property.GetValue(AutosarApplication.GetInstance()) as IAutosarTreeList;

                foreach (IGUID guid in treeList.GetItems())
                {
                    if (DoesGuidExists(treeList,guidObject) == true)
                    {
                        return true;
                    }
                }
                    
            }
            return false;
        }

        bool DoesGuidExists(IAutosarTreeList treeList, IGUID guidToBeFound)
        {
            foreach(IGUID guid in treeList.GetItems())
            {
                if (guid.Equals(guidToBeFound))
                {
                    return true;
                }
                else
                {
                    List<IAutosarTreeList> lists = guid.GetLists();
                    foreach(IAutosarTreeList list in lists)
                    {
                        if (DoesGuidExists(list, guidToBeFound) == true)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void DeleteUnexists(List<PropertyInfo> properties)
        {            
            for (int i = Items.Count - 1; i >=0; i--)
            {
                DeleteUnexistsRecursively(this.Items[i] as TreeViewItem, properties);
            }
        }

        private void DeleteUnexistsRecursively(TreeViewItem item, List<PropertyInfo> properties)
        {
            IGUID guid = item.Tag as IGUID;
            if (guid != null)
            {
                if (DoesGuidExistInProperties(properties, guid) == false)
                {
                    TreeViewItem parent = item.Parent as TreeViewItem;
                    if (parent != null)
                    {
                        parent.Items.Remove(item);
                    }
                }
                else 
                {
                    for (int i = item.Items.Count - 1; i >= 0; i--)
                    {
                        DeleteUnexistsRecursively(item.Items[i] as TreeViewItem, properties);
                    }
                }
            }
        }
    }
}
