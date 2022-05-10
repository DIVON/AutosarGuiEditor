using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.AutosarInterfaces.ClientServer
{
    public class ClientServerOperationDirectionComboBoxData
    {
        public ClientServerOperationDirectionComboBoxData(int id, String dir)
        {
            this.ID = id;
            this.Direction = dir;
        }

        public int ID
        {
            set;
            get;
        }

        public String Direction
        {
            set;
            get;
        }
    }

    public class ClientServerOperationDirectionComboBoxDataList : List<ClientServerOperationDirectionComboBoxData>
    {

        private ClientServerOperationDirectionComboBoxDataList()
        {

        }

        private static ClientServerOperationDirectionComboBoxDataList instance;

        public static ClientServerOperationDirectionComboBoxDataList Instance()
        {
            if (instance == null)
            {
                instance = new ClientServerOperationDirectionComboBoxDataList();
                instance.Add(new ClientServerOperationDirectionComboBoxData(0, ClientServerOperationField.STR_CONST_VAL_CONST_REF));
                instance.Add(new ClientServerOperationDirectionComboBoxData(0, ClientServerOperationField.STR_CONST_VAL_REF));
                instance.Add(new ClientServerOperationDirectionComboBoxData(0, ClientServerOperationField.STR_CONST_VALUE));
                instance.Add(new ClientServerOperationDirectionComboBoxData(0, ClientServerOperationField.STR_VAL_CONST_REF));
                instance.Add(new ClientServerOperationDirectionComboBoxData(0, ClientServerOperationField.STR_VAL_REF));
                instance.Add(new ClientServerOperationDirectionComboBoxData(0, ClientServerOperationField.STR_VALUE));
            }
            return instance;
        }
    }
}
