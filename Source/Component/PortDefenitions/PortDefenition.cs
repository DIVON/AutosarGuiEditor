using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.SystemInterfaces;
using AutosarGuiEditor.Source.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.PortDefenitions
{
    public class PortDefenition : IGUID
    {
        public Guid InterfaceGUID;
        

        public PortDefenition()
        {
            
        }        

        public object InterfaceDatatype
        {
            get
            {
                return AutosarApplication.GetInstance().GetPortInterface(InterfaceGUID);
            }
        }

        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);

            String interfaceGuidString = XmlUtilits.GetFieldValue(xml, "InterfaceGUID", "");
            InterfaceGUID = GuidUtils.GetGuid(interfaceGuidString, false);
            String portTypeString = XmlUtilits.GetFieldValue(xml, "PortType", "");
            PortType = (PortType)Enum.Parse(typeof(PortType), portTypeString);
        }

        public override void WriteToXML(XElement root)
        {
            XElement xmldatatype = new XElement("PortDefenition");
            base.WriteToXML(xmldatatype);
            xmldatatype.Add(new XElement("InterfaceGUID", InterfaceGUID.ToString("B")));
            xmldatatype.Add(new XElement("PortType", PortType.ToString()));
            root.Add(xmldatatype);
        }

        public String InterfaceName
        {
            get
            {
                String interfaceName = "ERROR";

                if (!InterfaceGUID.Equals(Guid.Empty))
                {
                    if ((this.portType == PortType.Sender) || (this.portType == PortType.Receiver))
                    {
                        SenderReceiverInterface srInterface = AutosarApplication.GetInstance().SenderReceiverInterfaces.FindObject(this.InterfaceGUID);
                        if (srInterface != null)
                        {
                            interfaceName = srInterface.Name;
                        }
                    }
                    else if ((this.portType == PortType.Client) || (this.portType == PortType.Server))
                    {
                        ClientServerInterface csInterface = AutosarApplication.GetInstance().ClientServerInterfaces.FindObject(this.InterfaceGUID);
                        if (csInterface != null)
                        {
                            interfaceName = csInterface.Name;
                        }
                    }
                }
                if (interfaceName == null)
                {
                    interfaceName = AutosarApplication.ErrorDataType;
                }
                return interfaceName;
            }
        }

        PortType portType = PortType.Unknown;
        public PortType PortType
        {
            get 
            {
                return portType;
            }
            set
            {
                portType = value;
            }       
        }

        public String InterfaceType
        {
            get
            {
                switch (portType)
                {
                    case PortType.Client:
                    {
                        return "Client";
                    }
                    case PortType.Receiver:
                    {
                        return "Receiver";
                    }
                    case PortType.Sender:
                    {
                        return "Sender";
                    }
                    case PortType.Server:
                    {
                        return "Server";
                    }
                }
                return "ERROR";
            }
        }
    }
}
