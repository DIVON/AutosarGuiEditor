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
                        case ClientServerOperationDirection.IN:
                        {
                            return "in";
                        }

                        case ClientServerOperationDirection.OUT:
                        {
                            return "out";
                        }
                        case ClientServerOperationDirection.INOUT:
                        {
                            return "in-out";
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
                    if (value.Equals("in"))
                    {
                        field.Direction = ClientServerOperationDirection.IN;
                    }
                    else if (value.Equals("out"))
                    {
                        field.Direction = ClientServerOperationDirection.OUT;
                    }
                    else if (value.Equals("in-out"))
                    {
                        field.Direction = ClientServerOperationDirection.INOUT;
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
            this.Add("in");
            this.Add("out");
            this.Add("in-out");
        }
    }
}
