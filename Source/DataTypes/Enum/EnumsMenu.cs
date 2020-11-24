using AutosarGuiEditor.Source.Forms.Controls;
using AutosarGuiEditor.Source.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AutosarGuiEditor.Source.DataTypes.Enum
{
    class EnumsMenu
    {
        AllowUpdater allowUpdater = new AllowUpdater();
        DataGrid grid;
        AutosarTreeViewControl tree;
        EnumDataType datatype;
        TextBox nameTextBox;

		public EnumDataType DataType
        {
            set
            {
                allowUpdater.StopUpdate();

                datatype = value;
                datatype.Fields.SortById();

                nameTextBox.Text = datatype.Name;

                RefreshGridView();

                allowUpdater.AllowUpdate();
            }
            get
            {
                return datatype;
            }
        }

        public EnumsMenu(AutosarTreeViewControl AutosarTree, DataGrid grid, TextBox nameTextBox)
        {
            this.tree = AutosarTree;
            this.grid = grid;
            this.nameTextBox = nameTextBox;
            this.nameTextBox.TextChanged += nameTextBox_TextChanged;
		}

        void nameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (allowUpdater.IsUpdateAllowed)
            {

                String newName = (sender as TextBox).Text;
                if (NameUtils.CheckComponentName(newName))
                {
                    datatype.Name = newName;

                    TreeViewItem item = tree.GetItem(datatype);
                    if (item != null)
                    {
                        item.Header = datatype.Name;
                    }
                }
            }
        }

		public void AddField()
        {
            datatype.Fields.Add(new EnumField());
            tree.UpdateAutosarTreeView(tree.SelectedItem);
		}		


        public void RefreshGridView()
        {
            grid.ItemsSource = null;
            grid.ItemsSource = datatype.Fields;
        }

        public void ChangeFieldValue(TextBox textBox)
        {
            int index = grid.SelectedIndex;
            if ((index < grid.Items.Count) && (index >= 0))
            {
                int val;

                if (int.TryParse(textBox.Text, out val))
                {
                    datatype.Fields[index].Value = val;
                }
                else
                {
                    textBox.Text = datatype.Fields[index].Value.ToString();
                    textBox.Select(textBox.Text.Length, 0);                    
                }                
            }
        }

        public void DeleteField()
        {
            int index = grid.SelectedIndex;
            if ((index < grid.Items.Count) && (index >= 0))
            {
                datatype.Fields.RemoveAt(index);
                RefreshGridView();
                tree.UpdateAutosarTreeView(tree.SelectedItem);
            }
        }

        public void RenameFieldName(TextBox textBox)
        {
            int index = grid.SelectedIndex;
            if ((index < grid.Items.Count) && (index >= 0))
            {
                if (NameUtils.CheckMacroName(textBox.Text))
                {
                    datatype.Fields[index].Name = textBox.Text;                    
                }
                else
                {
                    textBox.Text = datatype.Fields[index].Name;
                    textBox.Select(textBox.Text.Length, 0);
                }
                tree.UpdateAutosarTreeView(tree.SelectedItem);
            }
        }
	}    
}
