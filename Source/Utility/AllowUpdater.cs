using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.Utility
{
    public class AllowUpdater
    {
        bool allow = false;
        public void AllowUpdate()
        {
            allow = true;
        }
        public void StopUpdate()
        {
            allow = false;
        }
        public Boolean IsUpdateAllowed
        {
            get
            {
                return allow;
            }
        }
    }
}
