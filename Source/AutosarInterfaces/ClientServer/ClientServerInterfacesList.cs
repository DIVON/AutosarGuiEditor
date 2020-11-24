using AutosarGuiEditor.Source.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.AutosarInterfaces.ClientServer
{
    public class ClientServerInterfacesList : IGuidList<ClientServerInterface>
    {
        public override String GetName()
        {
            return "Client-Server interfaces";
        }
    }
}
