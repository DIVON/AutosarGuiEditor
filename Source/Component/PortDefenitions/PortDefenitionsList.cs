using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;
using AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver;
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

        public PortDefenitionsList PortsWithSenderReceiverInterface()
        {
            PortDefenitionsList ports = new PortDefenitionsList();
            foreach (PortDefenition portDef in this)
            {
                if ((portDef.PortType == PortType.Sender) || (portDef.PortType == PortType.Receiver))
                {
                    ports.Add(portDef);
                }
            }
            return ports;
        }

        public SenderReceiverInterfacesList UsedReceiverProviderInterfaces()
        {
            SenderReceiverInterfacesList uniqueRPinterfaces = new SenderReceiverInterfacesList();
            PortDefenitionsList allRPports = PortsWithSenderReceiverInterface();

            /*Remove duplicates*/
            var set = new HashSet<PortDefenition>();
            for (int i = 0; i < allRPports.Count; i++)
            {
                // Add if needed.
                if (!set.Contains(allRPports[i].InterfaceDatatype))
                {
                    uniqueRPinterfaces.Add(allRPports[i].InterfaceDatatype as SenderReceiverInterface);
                    set.Add(allRPports[i]);
                }
            }
            return uniqueRPinterfaces;
        }


        public PortDefenitionsList PortsWithClientInterface()
        {
            PortDefenitionsList ports = new PortDefenitionsList();
            foreach (PortDefenition portDef in this)
            {
                if (portDef.PortType == PortType.Client)
                {
                    ports.Add(portDef);
                }
            }
            return ports;
        }

        public ClientServerInterfacesList UsedClientInterfaces()
        {
            ClientServerInterfacesList uniqueInterfaces = new ClientServerInterfacesList();
            PortDefenitionsList allClientPorts = PortsWithClientInterface();

            /* Remove duplicates */
            var set = new HashSet<PortDefenition>();
            for (int i = 0; i < allClientPorts.Count; i++)
            {
                // Add if needed.
                if (!set.Contains(allClientPorts[i].InterfaceDatatype))
                {
                    uniqueInterfaces.Add(allClientPorts[i].InterfaceDatatype as ClientServerInterface);
                    set.Add(allClientPorts[i]);
                }
            }
            return uniqueInterfaces;
        }
    }
}
