using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Media.Imaging;
using AutosarGuiEditor.Source.Painters.PortsPainters;
using AutosarGuiEditor.Source.PortDefenitions;
using AutosarGuiEditor.Source.Painters;
using AutosarGuiEditor.Source.DataTypes.ComplexDataType;
using AutosarGuiEditor.Source.Composition;
using AutosarGuiEditor.Source.DataTypes.Enum;
using AutosarGuiEditor.Source.DataTypes.BaseDataType;
using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Painters.Components.Runables;
using AutosarGuiEditor.Source.Painters.Components.CData;
using AutosarGuiEditor.Source.Component.CData;
using AutosarGuiEditor.Source.Component.PerInstanceMemory;
using AutosarGuiEditor.Source.Painters.Components.PerInstance;
using AutosarGuiEditor.Source.Autosar.OsTasks;
using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;

namespace WpfTreeView
{
    /// <summary>
    /// Converts a full path to a specific image type of a drive, folder or file
    /// </summary>
    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public class TreeViewImagesConverter : IValueConverter
    {
        public static TreeViewImagesConverter Instance = new TreeViewImagesConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (value is ComponentInstance)
                {
                    //var image = "../../Images/icons/tree/Component.png";

                    var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\tree\\Component.png"; 
                    return new BitmapImage(new Uri(image));
                   /* return new BitmapImage(string.Format(
                        "pack://application:,,,/{0};component/Images/logo.ico",
                        Path.GetFileNameWithoutExtension(Application.ResourceAssembly.Location)));*/
                }
                else
                if (value is PortPainter)
                {
                    PortPainter portPainter = value as PortPainter;
                    if (portPainter.PortType == PortType.Server)
                    {
                        var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\ports\\ServerPort.png";
                        return new BitmapImage(new Uri(image));
                    }
                    if (portPainter.PortType == PortType.Client)
                    {
                        var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\ports\\ClientPort.png";
                        return new BitmapImage(new Uri(image));
                    }
                    if (portPainter.PortType == PortType.Receiver)
                    {
                        var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\ports\\ReceiverPort.png";
                        return new BitmapImage(new Uri(image));
                    }
                    if (portPainter.PortType == PortType.Sender)
                    {
                        var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\ports\\TransmitterPort.png";
                        return new BitmapImage(new Uri(image));
                    }
                }
                else 
                if (value is PortDefenition)
                {
                    PortDefenition portDefenition = value as PortDefenition;
                    if (portDefenition.PortType == PortType.Server)
                    {
                        var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\ports\\ServerPort.png";
                        return new BitmapImage(new Uri(image));
                    }
                    if (portDefenition.PortType == PortType.Client)
                    {
                        var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\ports\\ClientPort.png";
                        return new BitmapImage(new Uri(image));
                    }
                    if (portDefenition.PortType == PortType.Receiver)
                    {
                        var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\ports\\ReceiverPort.png";
                        return new BitmapImage(new Uri(image));
                    }
                    if (portDefenition.PortType == PortType.Sender)
                    {
                        var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\ports\\TransmitterPort.png";
                        return new BitmapImage(new Uri(image));
                    }
                }
                else
                if (value is PortConnection)
                {
                    var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\tree\\Connection.png";
                    return new BitmapImage(new Uri(image));
                }
                if (value is ComplexDataType)
                {
                    var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\tree\\composite data types.png";
                    return new BitmapImage(new Uri(image));
                }
                if (value is SimpleDataType)
                {
                    var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\tree\\Brick.png";
                    return new BitmapImage(new Uri(image));
                }
                if (value is EnumDataType)
                {
                    var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\tree\\Simple data type.png";
                    return new BitmapImage(new Uri(image));
                }
                if (value is BaseDataType)
                {
                    var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\tree\\Simple data type.png";
                    return new BitmapImage(new Uri(image));
                }
                if (value is CompositionInstance)
                {
                    var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\tree\\composition.png";
                    return new BitmapImage(new Uri(image));
                }
                if (value is ClientServerInterface)
                {
                    var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\tree\\Port.png";
                    return new BitmapImage(new Uri(image));
                }
                if (value is ClientServerOperationFieldsList)
                {
                    var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\tree\\Bricks.png";
                    return new BitmapImage(new Uri(image));
                }
                if (value is ClientServerOperationField)
                {
                    var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\tree\\Brick.png";
                    return new BitmapImage(new Uri(image));
                }
                if (value is SenderReceiverInterface)
                {
                    var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\tree\\dataflow.png";
                    return new BitmapImage(new Uri(image));
                }
                if (value is SenderReceiverInterfaceField)
                {
                    var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\tree\\Brick.png";
                    return new BitmapImage(new Uri(image));
                }
                if (value is SenderReceiverInterfaceFieldsList)
                {
                    var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\tree\\Bricks.png";
                    return new BitmapImage(new Uri(image));
                }
                if (value is ApplicationSwComponentType)
                {
                    var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\tree\\ComponentDefenition.png";
                    return new BitmapImage(new Uri(image));
                }
                if ((value is PeriodicRunnableInstance) || (value is PeriodicRunnableDefenition))
                {
                    var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\tree\\PeriodicRunnable.png";
                    return new BitmapImage(new Uri(image));
                }

                if ((value is CDataInstance) || (value is CDataDefenition))
                {
                    var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\tree\\CData.png";
                    return new BitmapImage(new Uri(image));
                }
                if ((value is PimInstance) || (value is PimDefenition))
                {
                    var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\tree\\Pim.png";
                    return new BitmapImage(new Uri(image));
                }
                if (value is OsTask)
                {
                    var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\tree\\osTask.jpg";
                    return new BitmapImage(new Uri(image));
                }
                if (value is ComplexDataTypeField)
                {
                    var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\tree\\Brick.png";
                    return new BitmapImage(new Uri(image));
                }
                if (value is EnumField)
                {
                    var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\tree\\Brick2.png";
                    return new BitmapImage(new Uri(image));
                }
                if (value is ClientServerOperation)
                {
                    var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\tree\\operation.png";
                    return new BitmapImage(new Uri(image));
                }
                if (value is ClientServerOperationList)
                {
                    var image = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Images\\icons\\tree\\gears.png";
                    return new BitmapImage(new Uri(image));
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
