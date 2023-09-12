using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;
using AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.RteGenerator.CppLang
{
    public class RtePortInterfaceGenerator
    {
        public void GeneratePortInterfaces(String folder)
        {
            String filename = folder + "\\" + Properties.Resources.RTE_PORT_INTERFACES_HPP_FILENAME;
            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator_Cpp.GenerateFileTitle(writer, filename, "Implementation for RTE connections source file");

            /* Add #include */
            RteFunctionsGenerator_Cpp.OpenGuardDefine(writer);
            RteFunctionsGenerator_Cpp.AddInclude(writer, "<string.h>");
            RteFunctionsGenerator_Cpp.AddInclude(writer, "<functional>");
            RteFunctionsGenerator_Cpp.AddInclude(writer, Properties.Resources.RTE_DATATYPES_HPP_FILENAME);

            /* Include */
            writer.WriteLine("");  
            
            foreach (SenderReceiverInterface srInterface in AutosarApplication.GetInstance().SenderReceiverInterfaces)
            {
                writer.WriteLine("/*************************************************************\n" +
                                  "* BEGIN Port Data Structure Definition: " + srInterface.Name + "\n" +
                                  " *************************************************************/");
                writer.WriteLine();

                WriteSenderInterfaceClass(writer, srInterface);
                writer.WriteLine();

                WriteReceiverInterfaceClass(writer, srInterface);
                writer.WriteLine();

                WriteProviderPortClass(writer, srInterface);
                writer.WriteLine();

                WriteReceiverPortClass(writer, srInterface);
                writer.WriteLine();

                writer.WriteLine("/*************************************************************\n" +
                                  "* END Port Data Structure Definition: " + srInterface.Name + "\n" +
                                  " *************************************************************/");
            }
            
            writer.WriteLine();

            foreach (ClientServerInterface csInterface in AutosarApplication.GetInstance().ClientServerInterfaces)
            {
                writer.WriteLine("/*************************************************************\n" +
                                  "* BEGIN Port Data Structure Definition: " + csInterface.Name + "\n" +
                                  " *************************************************************/");
                writer.WriteLine();

                WriteClientInterfaceClass(writer, csInterface);
                writer.WriteLine();

                WriteClientPortClass(writer, csInterface);
                writer.WriteLine();

                writer.WriteLine("/*************************************************************\n" +
                                  "* END Port Data Structure Definition: " + csInterface.Name + "\n" +
                                  " *************************************************************/");
            }
            RteFunctionsGenerator_Cpp.CloseGuardDefine(writer);
            writer.Close();
        }

        void WriteSenderInterfaceClass(StreamWriter writer, SenderReceiverInterface srInterface)
        {
            writer.WriteLine("/* Write functions */");
            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
            {
                String args = RteFunctionsGenerator_Cpp.GenerateSenderReceiverInterfaceArguments(field, PortDefenitions.PortType.Sender);
                writer.WriteLine("using " + GenerateWriteFuncName(srInterface, field) + " = Std_ReturnType(*)" + args + " ;");
            }
        }

        void WriteReceiverInterfaceClass(StreamWriter writer, SenderReceiverInterface srInterface)
        {
            writer.WriteLine("/* Read functions */");
            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
            {
                String args = RteFunctionsGenerator_Cpp.GenerateSenderReceiverInterfaceArguments(field, PortDefenitions.PortType.Receiver);
                writer.WriteLine("using " + GenerateReadFuncName(srInterface, field) + " = Std_ReturnType(*)" + args + " ;");
            }
        }

        void WriteClientInterfaceClass(StreamWriter writer, ClientServerInterface csInterface)
        {
            writer.WriteLine("/* Client-Server operations */");
            foreach (ClientServerOperation csOperation in csInterface.Operations)
            {
                String arguments = RteFunctionsGenerator_Cpp.GenerateClientServerInterfaceArguments(csOperation);
                writer.WriteLine("using " + GenerateClientFuncName(csInterface,csOperation) + " = Std_ReturnType(*)(" + arguments + ");");
            }
        }

        void WriteProviderPortClass(StreamWriter writer, SenderReceiverInterface srInterface)
        {
            writer.WriteLine("/* Writing port */");
            writer.WriteLine();

            String portDataStructure = RteFunctionsGenerator_Cpp.GeneratePortDataStructureDefenition(srInterface, PortDefenitions.PortType.Sender);
            writer.WriteLine("struct " + portDataStructure);
            writer.WriteLine("{");
            
            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
            {
                writer.WriteLine("    " + GenerateWriteFuncName(srInterface, field) + " Write_" + field.Name + ";");
            }

            writer.WriteLine("};");
        }

        void WriteReceiverPortClass(StreamWriter writer, SenderReceiverInterface srInterface)
        {
            writer.WriteLine("/* Reading port */");
            writer.WriteLine();

            String portDataStructure = RteFunctionsGenerator_Cpp.GeneratePortDataStructureDefenition(srInterface, PortDefenitions.PortType.Receiver);
            writer.WriteLine("struct " + portDataStructure);            
            writer.WriteLine("{");

            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
            {
                writer.WriteLine("    " + GenerateReadFuncName(srInterface, field) + " Read_" + field.Name + ";");
            }

            writer.WriteLine("};");
        }

        void WriteClientPortClass(StreamWriter writer, ClientServerInterface csInterface)
        {
            writer.WriteLine("/* Client port */");
            writer.WriteLine();

            String portDataStructure = RteFunctionsGenerator_Cpp.GeneratePortDataStructureDefenition(csInterface);
            writer.WriteLine("struct " + portDataStructure);
            writer.WriteLine("{");

            foreach (ClientServerOperation csOperation in csInterface.Operations)
            {
                writer.WriteLine("    " + GenerateClientFuncName(csInterface, csOperation) + " " + csOperation.Name + ";");
            }

            writer.WriteLine("};");
        }

        String GenerateReadFuncName(SenderReceiverInterface srInterface, SenderReceiverInterfaceField field)
        {
            return srInterface.Name + "_" + field.Name + "_ReadFunc";
        }

        String GenerateWriteFuncName(SenderReceiverInterface srInterface, SenderReceiverInterfaceField field)
        {
            return srInterface.Name + "_" + field.Name + "_WriteFunc";
        }

        String GenerateClientFuncName(ClientServerInterface csInterface, ClientServerOperation csOperation)
        {
            return csInterface.Name + "_" + csOperation.Name + "_Func";
        }
    }
}
