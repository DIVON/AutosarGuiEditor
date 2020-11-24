using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.Utility
{
    public static class GuidUtils
    {
        public static Guid GetGuid(String str, Boolean createNew)
        {
            Guid guid;
            if (Guid.TryParse(str, out guid))
            {
                return guid;
            }
            else
            {
                if (createNew)
                {
                    return Guid.NewGuid();
                }
                else
                {
                    return Guid.Empty;
                }
            }
        }
    }
}
