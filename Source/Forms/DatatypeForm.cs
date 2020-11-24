using AutosarGuiEditor.Source.SystemInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutosarGuiEditor.Source.Forms
{
    public partial class DatatypeForm : Form
    {
        DatatypeForm()
        {
            InitializeComponent();
            ShowOnlySimpleDataTypes = false;
        }

        static DatatypeForm instance = null;
        static public DatatypeForm GetInstance()
        {
            if (instance == null)
            {
                instance = new DatatypeForm();
            }
            return instance;
        }

        public List<IGUID> datatypes;
        public List<IGUID> filteredDatatypes = new List<IGUID>();

        public Boolean ShowOnlySimpleDataTypes
        {
            set;
            get;
        }

        private void DatatypeForm_Load(object sender, EventArgs e)
        {

        }


        public DialogResult ShowForm(List<IGUID> datatypes, Guid selectedDatatype)
        {
            this.datatypes = datatypes;
            filteredDatatypes.Clear();
            filteredDatatypes.AddRange(datatypes);

            datatypes.Sort(delegate(IGUID x, IGUID y)
            {
                if (x.Name == null && y.Name == null) return 0;
                else if (x.Name == null) return 1;
                else if (y.Name == null) return -1;
                else return x.Name.ToLower().CompareTo(y.Name.ToLower());
            });

            datatypesListBox.Items.Clear();
            if (datatypes != null)
            {
                foreach (IGUID datatype in datatypes)
                {
                    datatypesListBox.Items.Add(datatype);
                }                
            }

            for (int i = 0; i < datatypesListBox.Items.Count; i++)
            {
                if (datatypes[i].GUID.Equals(selectedDatatype))
                {
                    datatypesListBox.SelectedIndex = i;
                    break;
                }
            }
            filterTextBox.Select(filterTextBox.Text.Length, 0);
            
            return this.ShowDialog();
        }

        private void DatatypeForm_Shown(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        public IGUID SelectedDatatype = null;

        private void datatypesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedDatatype = datatypesListBox.SelectedItem as IGUID;
        }

        private void datatypesListBox_DoubleClick(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }


        private void filterTextBox_TextChanged(object sender, EventArgs e)
        {
            String filter = filterTextBox.Text.ToUpper();

            datatypesListBox.BeginUpdate();
            datatypesListBox.Items.Clear();
            if (datatypes != null)
            {
                foreach (IGUID datatype in datatypes)
                {
                    if (datatype.Name.ToUpper().Contains(filter))
                    {
                        datatypesListBox.Items.Add(datatype);
                    }
                }
            }
            datatypesListBox.EndUpdate();
        }

        private void DatatypeForm_Activated(object sender, EventArgs e)
        {
            ActiveControl = filterTextBox;
        }
    }
}
