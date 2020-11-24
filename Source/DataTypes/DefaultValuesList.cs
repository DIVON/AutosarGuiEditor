using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.DataTypes
{
    public class DefaultValue 
    {
        String fieldName = "";
        
        public String FieldName
        {
            set {
                fieldName = value;
            }
            get{
                return fieldName;
            }
        }

        String defaultValue = "";
        
        public String Value
        {
            set 
            {
                defaultValue = value;
            }
            get{
                return defaultValue;
            }
        }

    }

    public class DefaultValuesList : List<DefaultValue>
    {

    }
}
