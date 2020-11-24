using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace AutosarGuiEditor.Source.Controllers
{
    class ComplexDataTypeGridViewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ComplexDataTypeField)
            {
                ComplexDataTypeField field = (value as ComplexDataTypeField);
                //if (field.IsArray)
                //{
                //    return Visibility.Visible;
                //}
                //else
                //{
                //    return Visibility.Hidden;
                //}
            }

            return Visibility.Hidden;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
