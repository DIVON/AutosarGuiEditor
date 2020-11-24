using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.Fabrics
{
    public class PortConnectionFabric
    {
        PortConnectionFabric ()
        {

        }

        private static PortConnectionFabric instance = null;
        public static PortConnectionFabric GetInstance()
        {
            if (instance == null)
            {
                instance = new PortConnectionFabric();
            }
            return instance;
        }

        public PortConnection CreatePortConnection()
        {
            PortConnection connection = new PortConnection();
            return connection;
        }
    }
}
