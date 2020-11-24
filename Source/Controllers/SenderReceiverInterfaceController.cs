﻿using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver;
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
    public class SenderReceiverInterfaceController
    {
        AllowUpdater allowUpdater = new AllowUpdater();
        AutosarTreeViewControl tree;
        DataGrid grid;
        TextBox nameTextBox;

        public SenderReceiverInterfaceController(AutosarTreeViewControl AutosarTree, DataGrid grid, TextBox nameTextBox)
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
                    srInterface.Name = newName;

                    TreeViewItem item = tree.GetItem(srInterface);
                    if (item != null)
                    {
                        item.Header = srInterface.Name;
                    }
                }
            }
        }

        public SenderReceiverInterface CreateSenderReceiverInterface()
        {
            string templateName = "iSenderReceiver";
            if (AutosarApplication.GetInstance().SenderReceiverInterfaces.FindObject(templateName) != null)
            {
                int index = 0;
                while (AutosarApplication.GetInstance().SenderReceiverInterfaces.FindObject(templateName) != null)
                {
                    index++;
                    templateName = "iSenderReceiver" + index.ToString();
                }
            }

            SenderReceiverInterface srInterface = DataTypeFabric.Instance().CreateSenderReceiverInterface(templateName);
            AutosarApplication.GetInstance().SenderReceiverInterfaces.Add(srInterface);
            return srInterface;
        }

        SenderReceiverInterface _srInterface;
        public SenderReceiverInterface srInterface
        {
            set
            {
                _srInterface = value;
                allowUpdater.StopUpdate();
                nameTextBox.Text = _srInterface.Name;
                RefreshGridView();
                allowUpdater.AllowUpdate();
            }
            get
            {
                return _srInterface;
            }
        }

        public void AddField()
        {
            srInterface.Fields.Add(new SenderReceiverInterfaceField());
            RefreshGridView();
        }

        public void RefreshGridView()
        {
            grid.ItemsSource = null;
            grid.ItemsSource = srInterface.Fields;
        }

        public void IsPointerCheckBoxClick(CheckBox sender)
        {
            int index = grid.SelectedIndex;
            if ((index < grid.Items.Count) && (index >= 0))
            {
                srInterface.Fields[index].IsPointer = (sender.IsChecked == true);
                RefreshGridView();
            }
        }

        public void ChangeDatatypeButtonClick()
        {
            int index = grid.SelectedIndex;

            List<IGUID> datatypes = AutosarApplication.GetInstance().GetAllDataTypes(srInterface.Name);
            System.Windows.Forms.DialogResult result = DatatypeForm.GetInstance().ShowForm(datatypes, srInterface.Fields[index].BaseDataTypeGUID);
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                srInterface.Fields[index].BaseDataTypeGUID = DatatypeForm.GetInstance().SelectedDatatype.GUID;
                RefreshGridView();
            }
        }

        public void DeleteButtonClick()
        {
            int index = grid.SelectedIndex;
            if ((index < grid.Items.Count) && (index >= 0))
            {
                srInterface.Fields.RemoveAt(index);
                RefreshGridView();
            }
        }

        public void RenameFieldTextEdit(TextBox textbox)
        {
            int index = grid.SelectedIndex;
            if ((index < grid.Items.Count) && (index >= 0))
            {
                srInterface.Fields[index].Name = textbox.Text;
            }
        }
    }
}
