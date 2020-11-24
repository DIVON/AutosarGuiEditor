using AutosarGuiEditor.Source.App.Settings;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.Controllers
{
    public class OpenSaveController
    {
        public AutosarApplication autosarApp;
        public OpenSaveController (AutosarApplication autosarApp)
        {
            this.autosarApp = autosarApp;
        }

        public String FileName = "";

        public bool Save()
        {
            if (FileName.Length == 0)
            {
                SaveFileDialog dialog = new SaveFileDialog()
                {
                    Filter="(*.age)|*.age"
                };

                if (dialog.ShowDialog() == true)
                {
                    FileName = dialog.FileName;
                    return autosarApp.SaveToFile(dialog.FileName);
                }
            }
            else
            {
                return autosarApp.SaveToFile(FileName);
            }
            return false;
        }

        public bool SaveAs()
        {
            SaveFileDialog dialog = new SaveFileDialog()
            {
                Filter = "(*.age)|*.age"
            };

            if (dialog.ShowDialog() == true)
            {
                FileName = dialog.FileName;
                Boolean saved = autosarApp.SaveToFile(dialog.FileName);
                if (saved)
                {
                    SettingsProvider.GetInstance().LastProjectFileName = FileName;
                }
                return saved;
            }
            return false;
        }

        public bool Open()
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = "(*.age)|*.age"
            };
            if (dialog.ShowDialog() == true)
            {
                bool opened = autosarApp.LoadFromFile(dialog.FileName);
                if (opened)
                {
                    FileName = dialog.FileName;
                    SettingsProvider.GetInstance().LastProjectFileName = FileName;
                }
                return opened;
            }
            return false;
        }

        public void Clear()
        {
            FileName = "";
        }
    }
}
