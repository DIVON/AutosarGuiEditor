using AutosarGuiEditor.Source.RteGenerator;
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

namespace AutosarGuiEditor.Source.Forms
{
    /// <summary>
    /// Interaction logic for RteGenerationForm.xaml
    /// </summary>
    public partial class ProjectSettingsForm : Window
    {
        public ProjectSettingsForm()
        {
            InitializeComponent();

            var mcuTypes = Enum.GetValues(typeof(MCUTypeDef));
            foreach (var mcuType in mcuTypes)
            {
                McuTypeComboBox.Items.Add(mcuType.ToString());
            }

            var programmingLanguages = Enum.GetValues(typeof(ProgrammingLanguageTypeDef));
            foreach (var pl in programmingLanguages)
            {
                ProgramLanguageComboBox.Items.Add(pl.ToString());
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        uint lastFrequency = 1000;
        public uint Frequency
        {
            set
            {
                lastFrequency = value;
                SysTickFrequencyEdit.Text = lastFrequency.ToString();
            }
            get
            {
                return lastFrequency;
            }
        }

        public String RteGenerationPath
        {
            set
            {
                RteGeneratingFolderEdit.Text = value;
            }
            get
            {
                return RteGeneratingFolderEdit.Text;
            }
        }

        public String TestRteGenerationPath
        {
            set
            {
                TestRteGeneratingFolderEdit.Text = value;
            }
            get
            {
                return TestRteGeneratingFolderEdit.Text;
            }
        }

        public String ComponentGenerationPath
        {
            set
            {
                ComponentsGeneratingFolderEdit.Text = value;
            }
            get
            {
                return ComponentsGeneratingFolderEdit.Text;
            }
        }

        
        private void SysTickFrequencyEdit_TextChanged(object sender, TextChangedEventArgs e)
        {
            uint newFreq = 0;
            if (uint.TryParse((sender as TextBox).Text, out newFreq))
            {
                lastFrequency = newFreq;
            }
            else
            {
                (sender as TextBox).Text = lastFrequency.ToString();
                (sender as TextBox).CaretIndex = (sender as TextBox).Text.Length;
            }
        }

        private void OpenDirDialogButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.RootFolder = Environment.SpecialFolder.MyComputer;
                dialog.SelectedPath = ComponentsGeneratingFolderEdit.Text;
                
                dialog.ShowNewFolderButton = true;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    ComponentsGeneratingFolderEdit.Text = dialog.SelectedPath;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void OpenRteDirDialogButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.RootFolder = Environment.SpecialFolder.MyComputer;
                dialog.SelectedPath = RteGeneratingFolderEdit.Text;

                dialog.ShowNewFolderButton = true;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    RteGeneratingFolderEdit.Text = dialog.SelectedPath;
                }
            }
        }

        private void OpenTestRteDirDialogButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.RootFolder = Environment.SpecialFolder.MyComputer;
                dialog.SelectedPath = TestRteGeneratingFolderEdit.Text;

                dialog.ShowNewFolderButton = true;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    TestRteGeneratingFolderEdit.Text = dialog.SelectedPath;
                }
            }
        }
    }
}
