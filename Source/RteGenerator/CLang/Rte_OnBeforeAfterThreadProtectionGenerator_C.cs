using AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Composition;
using AutosarGuiEditor.Source.Painters;
using AutosarGuiEditor.Source.Painters.PortsPainters;
using AutosarGuiEditor.Source.PortDefenitions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.RteGenerator.CLang
{
    class Rte_OnBeforeAfterThreadProtectionGenerator_C
    {
        public void GenerateThreadProtectionFunctions(String folder)
        {
            String filename = folder + "\\" + Properties.Resources.RTE_THREAD_PROTECTION_H_FILENAME;
            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator_C.GenerateFileTitle(writer, filename, "Implementation for interrupt protection of RTE.");

            /*Add #include */            
            RteFunctionsGenerator_C.AddInclude(writer, Properties.Resources.RTE_DATATYPES_H_FILENAME);
            
            /* Include */
            writer.WriteLine("");

            GenerateProtectionForWriteFunctions(writer);
            GenerateProtectionForReadFunctions(writer);
            GenerateProtectionForSendFunctions(writer);
            GenerateProtectionForReceiveFunctions(writer);

            writer.Close();
        }
        void GenerateProtectionForReceiveFunctions(StreamWriter writer)
        {
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;

                    foreach (PortDefenition portDef in compDef.Ports)
                    {
                        if ((portDef.PortType == PortType.Receiver) && ((portDef.InterfaceDatatype as SenderReceiverInterface).IsQueued == true))
                        {
                            SenderReceiverInterface srInterface = (portDef.InterfaceDatatype as SenderReceiverInterface);
                            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                            { 
                                if (srInterface.IsThreadIrqProtected == true)
                                {
                                    String RteFuncName = RteFunctionsGenerator_C.GenerateInternalSendReceiveConnectionFunctionName(component.Name, portDef, field);
                                    writer.WriteLine("void OnBefore_" + RteFuncName + "();");
                                    writer.WriteLine("void OnAfter_" + RteFuncName + "();");
                                    writer.WriteLine();
                                }
                            }
                        }
                    }
                }
            }
        }

        void GenerateProtectionForSendFunctions(StreamWriter writer)
        {
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;

                    foreach (PortDefenition portDef in compDef.Ports)
                    {
                        if ((portDef.PortType == PortType.Sender) && ((portDef.InterfaceDatatype as SenderReceiverInterface).IsQueued == true))
                        {
                            SenderReceiverInterface srInterface = (portDef.InterfaceDatatype as SenderReceiverInterface);
                            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                            {
                                String RteFuncName = RteFunctionsGenerator_C.GenerateInternalSendReceiveConnectionFunctionName(component.Name, portDef, field);

                                if (srInterface.IsThreadIrqProtected == true)
                                {
                                    writer.WriteLine("void OnBefore_" + RteFuncName + "();");
                                    writer.WriteLine("void OnAfter_" + RteFuncName + "();");
                                    writer.WriteLine();
                                }
                            }
                        }
                    }
                }
            }
        }

        void GenerateProtectionForWriteFunctions(StreamWriter writer)
        {
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;

                    foreach (PortDefenition portDef in compDef.Ports)
                    {
                        if ((portDef.PortType == PortType.Sender) && ((portDef.InterfaceDatatype as SenderReceiverInterface).IsQueued == false))
                        {
                            SenderReceiverInterface srInterface = (portDef.InterfaceDatatype as SenderReceiverInterface);
                            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                            {
                                String RteFuncName = RteFunctionsGenerator_C.GenerateInternalReadWriteConnectionFunctionName(component.Name, portDef, field);

                                if (srInterface.IsThreadIrqProtected == true)
                                {
                                    writer.WriteLine("void OnBefore_" + RteFuncName + "();");
                                    writer.WriteLine("void OnAfter_" + RteFuncName + "();");
                                    writer.WriteLine();
                                }
                            }
                        }
                    }
                }
            }
        }

        void GenerateProtectionForReadFunctions(StreamWriter writer)
        {
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;

                    foreach (PortDefenition portDef in compDef.Ports)
                    {
                        if ((portDef.PortType == PortType.Receiver) && ((portDef.InterfaceDatatype as SenderReceiverInterface).IsQueued == false))
                        {
                            SenderReceiverInterface srInterface = (portDef.InterfaceDatatype as SenderReceiverInterface);
                            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                            {
                                String RteFuncName = RteFunctionsGenerator_C.GenerateInternalReadWriteConnectionFunctionName(component.Name, portDef, field);

                                if (srInterface.IsThreadIrqProtected == true)
                                {
                                    writer.WriteLine("void OnBefore_" + RteFuncName + "();");
                                    writer.WriteLine("void OnAfter_" + RteFuncName + "();");
                                    writer.WriteLine();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
