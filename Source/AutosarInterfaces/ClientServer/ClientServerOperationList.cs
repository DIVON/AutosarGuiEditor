using AutosarGuiEditor.Source.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.AutosarInterfaces.ClientServer
{
    public class ClientServerOperationList : IGuidList<ClientServerOperation>
    {
        public override String GetName()
        {
            return "Operations";
        }
    }
}
