using AutosarGuiEditor.Source.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver
{
    public class SenderReceiverInterfaceFieldsList : IGuidList<SenderReceiverInterfaceField>
    {
        public override String GetName()
        {
            return "Fields";
        }
    }
}
