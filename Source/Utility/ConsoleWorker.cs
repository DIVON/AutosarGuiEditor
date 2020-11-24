using AutosarGuiEditor.Source.Forms;
using AutosarGuiEditor.Source.Forms.ConsoleForm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.Utility
{
    public class ConsoleWorker
    {
        static ConsoleWorker worker;
        static ConsoleForm window;

        StringWriter writer = new StringWriter();

        ConsoleWorker()
        {
            window = new ConsoleForm();
        }

        public static ConsoleWorker GetInstance()
        {
            if (worker == null)
            {
                worker = new ConsoleWorker();
            }
            return worker;
        }

        public void Show()
        {
            window.Show();
        }

        public void AddText(String text)
        {
            writer.Write(text);
            window.SetString(writer.ToString());
        }

        public void Clear()
        {
            writer = new StringWriter();
        }        
    }
}
