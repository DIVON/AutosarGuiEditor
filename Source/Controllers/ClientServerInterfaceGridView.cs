using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace AutosarGuiEditor.Source.Controllers
{
    public class ClientServerInterfaceGridViewField
    {
        public ClientServerInterfaceGridViewField(Object obj)
        {
            this.containObject = obj;
        }

        public String Name
        {
            get
            {
                if (containObject is ClientServerOperation)
                {
                    return (containObject as ClientServerOperation).Name;
                }
                if (containObject is ClientServerOperationField)
                {
                    return (containObject as ClientServerOperationField).Name;
                }
                return "";
            }
            set
            {
                if (containObject is ClientServerOperation)
                {
                    (containObject as ClientServerOperation).Name = value;
                    return;
                }
                if (containObject is ClientServerOperationField)
                {
                    (containObject as ClientServerOperationField).Name = value;
                    return;
                }
            }
        }

        

        public String Direction
        {
            get
            {
                if (containObject is ClientServerOperationField)
                {
                    switch ((containObject as ClientServerOperationField).Direction)
                    {
                        case ClientServerOperationDirection.CONST_VAL_CONST_REF:
                        {
                            return ClientServerOperationField.STR_CONST_VAL_CONST_REF;
                        }
                        case ClientServerOperationDirection.CONST_VAL_REF:
                        {
                            return ClientServerOperationField.STR_CONST_VAL_REF;
                        }
                        case ClientServerOperationDirection.CONST_VALUE:
                        {
                            return ClientServerOperationField.STR_CONST_VALUE;
                        }
                        case ClientServerOperationDirection.VAL_CONST_REF:
                        {
                            return ClientServerOperationField.STR_VAL_CONST_REF;
                        }
                        case ClientServerOperationDirection.VAL_REF:
                        {
                            return ClientServerOperationField.STR_VAL_REF;
                        }
                        case ClientServerOperationDirection.VALUE:
                        {
                            return ClientServerOperationField.STR_VALUE;
                        }
                    }
                }
                return "";
            }
            set
            {
                if (containObject is ClientServerOperationField)
                {
                    ClientServerOperationField field = (containObject as ClientServerOperationField);
                    if (value.Equals(ClientServerOperationField.STR_CONST_VAL_CONST_REF))
                    {
                        field.Direction = ClientServerOperationDirection.CONST_VAL_CONST_REF;
                    }
                    else if (value.Equals(ClientServerOperationField.STR_CONST_VAL_REF))
                    {
                        field.Direction = ClientServerOperationDirection.CONST_VAL_REF;
                    }
                    else if (value.Equals(ClientServerOperationField.STR_CONST_VALUE))
                    {
                        field.Direction = ClientServerOperationDirection.CONST_VALUE;
                    }
                    else if (value.Equals(ClientServerOperationField.STR_VAL_CONST_REF))
                    {
                        field.Direction = ClientServerOperationDirection.VAL_CONST_REF;
                    }
                    else if (value.Equals(ClientServerOperationField.STR_VAL_REF))
                    {
                        field.Direction = ClientServerOperationDirection.VAL_REF;
                    }
                    else if (value.Equals(ClientServerOperationField.STR_VALUE))
                    {
                        field.Direction = ClientServerOperationDirection.VALUE;
                    }
                }
            }
        }

        public String DataType
        {
            get
            {
                if (containObject is ClientServerOperationField)
                {
                    ClientServerOperationField obj = (containObject as ClientServerOperationField);
                    return AutosarApplication.GetInstance().GetDataTypeName(obj.BaseDataTypeGUID);
                }
                return "";
            }
        }
        public String TextBoxMargin
        {
            get
            {
                if (containObject is ClientServerOperationField)
                {
                    return "30,0,0,0";
                }
                else
                {
                    return "0,0,0,0";
                }
            }
        }

        public Object containObject;
    }

    public class ClientServerGridViewObjectVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ClientServerInterfaceGridViewField)
            {
                ClientServerInterfaceGridViewField field = (value as ClientServerInterfaceGridViewField);
                if (field.containObject is ClientServerOperationField)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Hidden;
                }
            }

            return Visibility.Hidden;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ClientServerGridViewAddButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ClientServerInterfaceGridViewField)
            {
                ClientServerInterfaceGridViewField field = (value as ClientServerInterfaceGridViewField);
                if (field.containObject is ClientServerOperationField)
                {
                    return Visibility.Hidden;
                }
                else
                {
                    return Visibility.Visible;
                }
            }

            return Visibility.Hidden;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InOutDirectionList:List<String>
    {
        public InOutDirectionList()
        {
            this.Add(ClientServerOperationField.STR_CONST_VAL_CONST_REF);
            this.Add(ClientServerOperationField.STR_CONST_VAL_REF);
            this.Add(ClientServerOperationField.STR_CONST_VALUE);
            this.Add(ClientServerOperationField.STR_VAL_CONST_REF);
            this.Add(ClientServerOperationField.STR_VAL_REF);
            this.Add(ClientServerOperationField.STR_VALUE);
        }
    }
}
