using AutosarGuiEditor.Source.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.PortDefenitions
{
    public class PortDefenitionsList : IGuidList<PortDefenition>
    {
        public override String GetName()
        {
            return "Port defenitions";
        }

        public PortDefenitionsList PortsWithSenderInterface()
        {
            PortDefenitionsList ports = new PortDefenitionsList();
            foreach (PortDefenition portDef in this)
            {
                if (portDef.PortType == PortType.Sender)
                {
                    ports.Add(portDef);
                }
            }
            return ports;
        }
    }
}
