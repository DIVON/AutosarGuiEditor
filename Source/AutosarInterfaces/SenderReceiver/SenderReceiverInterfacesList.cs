using AutosarGuiEditor.Source.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver
{
    public class SenderReceiverInterfacesList : IGuidList<SenderReceiverInterface>
    {
        public override String GetName()
        {
            return "Sender-Receiver interfaces";
        }
    }
}
