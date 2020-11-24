using AutosarGuiEditor.Source.PortDefenitions;
using AutosarGuiEditor.Source.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace AutosarGuiEditor.Source.Forms
{
    public partial class AddPortForm : Form
    {
        private AddPortForm()
        {
            InitializeComponent();
        }

        static AddPortForm form;

        static public AddPortForm GetInstance()
        {
            if (form == null)
            {
                form = new AddPortForm();
            }
            return form;
        }

        PortsFabric portsFabric = new PortsFabric();

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            bool nameChecked = NameUtils.CheckComponentName(portNameTextBox.Text);
            if (nameChecked == false)
            {
                MessageBox.Show("Wrong port name", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK)
            {
                PortType type;

                if (clientPort.Checked)
                {
                    type = PortType.Client;
                } 
                else if (serverPort.Checked)
                {
                    type = PortType.Server;
                } 
                else if (TransmitterPort.Checked)
                {
                    type = PortType.Sender;
                } 
                else
                {
                    type = PortType.Receiver;
                }
                
                CreatedPortDefenition = portsFabric.CreatePortDefenition(portNameTextBox.Text, type);
            }
            else if (this.DialogResult == DialogResult.Cancel)
            {
                CreatedPortDefenition = null;
            }
        }

        public PortDefenition CreatedPortDefenition = null;

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
