using AutosarGuiEditor.Source.Autosar.OsTasks;
using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;
using AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Component.CData;
using AutosarGuiEditor.Source.Component.PerInstanceMemory;
using AutosarGuiEditor.Source.Composition;
using AutosarGuiEditor.Source.DataTypes.ArrayDataType;
using AutosarGuiEditor.Source.DataTypes.ComplexDataType;
using AutosarGuiEditor.Source.DataTypes.Enum;
using AutosarGuiEditor.Source.Interfaces;
using AutosarGuiEditor.Source.Painters;
using AutosarGuiEditor.Source.PortDefenitions;
using AutosarGuiEditor.Source.RteGenerator;
using AutosarGuiEditor.Source.SystemInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AutosarGuiEditor.Source.Tester
{
    public class ArxmlTester
    {
        private AutosarApplication autosarApp;
        private StringWriter writer;

        public ArxmlTester(AutosarApplication autosarApp, StringWriter writer)
        {
            this.autosarApp = autosarApp;
            this.writer = writer;
        }

        public void Test()
        {
            TestEnums();
            TestComplexDataTypes();
            TestClientServer();
            TestSenderReceiver();
            TestComponentDefenition();
            TestCompostions();
            TestComponentInstances();
            TestErrorNames();
            TestForComplexDependency();
            TestArrays();
            TestConnections();
            TestOsTasks();
        }

        const string ERROR = "ERROR: ";
        const string OK = "OK: ";

        void TestConnections()
        {
            foreach (CompositionInstance composition in autosarApp.Compositions)
            {
                foreach (PortConnection connection in composition.Connections)
                {
                    if (!connection.Port1.PortDefenition.InterfaceGUID.Equals(connection.Port2.PortDefenition.InterfaceGUID))
                    {
                        
                        //AppendText("Different datatypes in connection: " + connection.Component1.Name + "(" + connection.Port1.Name + ")        " + connection.Component2.Name + "(" + connection.Port2.Name + ")", Error: true);
                        AppendText("Different datatypes in " + composition.Name + " connection: " + connection.Name, Error: true);
                    }
                }                
            }
        }

        void TestArrays()
        {
            /* Check that arrays couldn't have arrays */
            foreach (ArrayDataType arrDt in autosarApp.ArrayDataTypes)
            {
                for(int i = 0; i <autosarApp.ArrayDataTypes.Count; i++)
                {
                    if (arrDt.DataTypeGUID.Equals(autosarApp.ArrayDataTypes[i].GUID))
                    {
                        AppendText("Arrays couldn't have other array datatype: " + arrDt.Name, Error: true); 
                    }
                }
            }

            foreach (ArrayDataType arrDt in autosarApp.ArrayDataTypes)
            {
                if (arrDt.DataTypeName.Equals("ERROR"))
                {
                    AppendText("Array " + arrDt.Name + " haven't datatype", Error: true);
                }
            }

            foreach (ArrayDataType arrDt in autosarApp.ArrayDataTypes)
            {
                if (arrDt.Size <= 0)
                {
                    AppendText("Array " + arrDt.Name + " have zero size!", Error: true);
                }
            }
        }

        void TestForComplexDependency()
        {
            ComplexDataTypesList allCDT = new ComplexDataTypesList();
            allCDT.Capacity = autosarApp.ComplexDataTypes.Count;
            allCDT.AddRange(autosarApp.ComplexDataTypes);
            RteDataTypesGenerator.RemoveDataTypesWithoutDependencies(allCDT);
            bool result = RteDataTypesGenerator.SortDependenciedCDT(allCDT);
            if (!result)
            {
                String strRes = "There are recursion dependencies in components: " + Environment.NewLine;
                foreach (ComplexDataType cdt in allCDT)
                {
                    strRes += cdt.Name + Environment.NewLine;
                }
                AppendText(strRes, Error:true);                
            }
        }


        protected void TestEnums()
        {
            /* Check if there are enums with similar names */
            CheckSimilarNames(autosarApp.ComplexDataTypes.ConvertAll(x => x as IGUID), "ENUM");

            /* Check each enum */
            foreach (EnumDataType enumdt in autosarApp.Enums)
            {
                TestEnum(enumdt);
            }
        }

        protected void TestEnum(EnumDataType enumDt)
        {
            bool withoutErrors = true;

            for (int i = 0; i < enumDt.Fields.Count - 1; i++)
            {
                for (int j = i + 1; j < enumDt.Fields.Count; j++)
                {
                    if (enumDt.Fields[i].Value == enumDt.Fields[j].Value)
                    {
                        withoutErrors = false;
                        AppendText(enumDt.Name + " field value " + enumDt.Fields[i].Name + "equals to " + enumDt.Fields[j].Name + " (" + enumDt.Fields[i].Value + ")", Error: true);
                    }
                    if (enumDt.Fields[i].Name == enumDt.Fields[j].Name)
                    {
                        withoutErrors = false;
                        AppendText(enumDt.Name + " has similar names (" + enumDt.Fields[i].Name + ") of fields for " + i.ToString() + " and " + j.ToString() + " indexes", Error: true);
                    }
                }
            }
            if (withoutErrors)
            {
                AppendText(enumDt.Name);
            }
        }

        protected void TestComplexDataTypes()
        {
            /* Check if there are similar names */
            CheckSimilarNames(autosarApp.ComplexDataTypes.ConvertAll(x => x as IGUID), "complex data type");

            /* Check each */
            foreach (ComplexDataType complexDt in autosarApp.ComplexDataTypes)
            {
                TestComplexDataType(complexDt);
            }
        }

        protected void TestComplexDataType(ComplexDataType elem)
        {
            bool withoutErrors = true;

            /* Check if fields have similar names */
            for (int i = 0; i < elem.Fields.Count - 1; i++)
            {
                for (int j = i + 1; j < elem.Fields.Count; j++)
                {
                    if (elem.Fields[i].Name == elem.Fields[j].Name)
                    {
                        withoutErrors = false;
                        AppendText(elem.Name + " has similar field names (" + elem.Fields[i].Name + ") of fields for " + i.ToString() + " and " + j.ToString() + " indexes", Error: true);
                    }
                }
            }

            /* CheckDatatype */
            foreach (ComplexDataTypeField field in elem.Fields)
            {
                if (field.DataTypeName.Equals(AutosarApplication.ErrorDataType))
                {
                    AppendText(elem.Name + " : " + field.Name + " doesn't have specified datatype ", Error: true);
                    withoutErrors = false;
                }
            }

            if (withoutErrors)
            {
                AppendText(elem.Name);
            }
        }

        protected void TestClientServer()
        {
            /* Check if there are similar names */
            CheckSimilarNames(autosarApp.ClientServerInterfaces.ConvertAll(x => x as IGUID), "Client-Server");

            /* Check each enum */
            foreach (ClientServerInterface elem in autosarApp.ClientServerInterfaces)
            {
                TestClientServer(elem);
            }
        }

        protected void TestClientServer(ClientServerInterface elem)
        {
            bool withoutErrors = true;

            if (elem.Operations.Count == 0)
            {
                AppendText(elem.Name + " :  does not have any operations!", Error: true);
            }

            /* Check if there is an operation with similar names */
            withoutErrors &= CheckSimilarNames(elem.Operations.ConvertAll(x => x as IGUID), elem.Name + ": operations");

            /* Check if there is an similar fields in an operation */
            foreach (ClientServerOperation operation in elem.Operations)
            {
                withoutErrors &= CheckSimilarNames(operation.Fields.ConvertAll(x => x as IGUID), elem.Name + ": operation fields");
            }
            
            /* CheckDatatype */
            foreach (ClientServerOperation operation in elem.Operations)
            {
                foreach(ClientServerOperationField field in operation.Fields)
                {
                    if (field.DataTypeName.Equals(AutosarApplication.ErrorDataType))
                    {
                        AppendText(elem.Name + " : " + operation.Name + " : " + field.Name + " doesn't have specified datatype ", Error: true);
                         withoutErrors = false;
                    }
                }                
            }

            if (withoutErrors)
            {
                AppendText(elem.Name);
            }
        } 


        protected void TestSenderReceiver()
        {
            /* Check if there are similar names */
            CheckSimilarNames(autosarApp.SenderReceiverInterfaces.ConvertAll(x => x as IGUID), "Sender-Receiver");

            /* Check each enum */
            foreach (SenderReceiverInterface elem in autosarApp.SenderReceiverInterfaces)
            {
                TestSenderReceiver(elem);
            }
        }

        protected void TestSenderReceiver(SenderReceiverInterface elem)
        {
            bool withoutErrors = true;

            if (elem.Fields.Count == 0)
            {
                AppendText(elem.Name + " :  does not have any fields!", Error: true);
            }

            
            /* Check if there is an fields with similar names */
            withoutErrors &= CheckSimilarNames(elem.Fields.ConvertAll(x => x as IGUID), elem.Name + ": fields");

            /* Check datatypes */

            foreach (SenderReceiverInterfaceField field in elem.Fields)
            {
                if (field.DataTypeName.Equals(AutosarApplication.ErrorDataType))
                {
                    AppendText(elem.Name + " : " + field.Name + " doesn't have specified datatype ", Error: true);
                    withoutErrors = false;
                }
            }


            if (withoutErrors)
            {
                AppendText(elem.Name);
            }
        }

        protected void TestComponentDefenition()
        {
            /* Check if there are similar names */
            CheckSimilarNames(autosarApp.ComponentDefenitionsList.ConvertAll(x => x as IGUID), "Component Defenition");

            /* Check each enum */
            foreach (ComponentDefenition elem in autosarApp.ComponentDefenitionsList)
            {
                TestComponentDefenition(elem);
            }
        }

        protected void TestComponentDefenition(ComponentDefenition elem)
        {
            bool withoutErrors = true;

     
            /* Check if there is an CDATA with similar names */
            withoutErrors &= CheckSimilarNames(elem.CDataDefenitions.ConvertAll(x => x as IGUID), elem.Name + ": CData ");

            /* Check if there is an PIM with similar names */
            withoutErrors &= CheckSimilarNames(elem.PerInstanceMemoryList.ConvertAll(x => x as IGUID), elem.Name + ": Pim ");

            /* Check if there is an Ports with similar names */
            withoutErrors &= CheckSimilarNames(elem.Ports.ConvertAll(x => x as IGUID), elem.Name + ": Port ");

            /* Check if there is an Ports with similar names */
            withoutErrors &= CheckSimilarNames(elem.Runnables.ConvertAll(x => x as IGUID), elem.Name + ": Runnables ");

            /* CData */
            foreach (CDataDefenition cdata in elem.CDataDefenitions)
            {
                if (cdata.DataTypeName.Equals(AutosarApplication.ErrorDataType))
                {
                    AppendText("Component: " + elem.Name + ", Cdata : " + cdata.Name + " doesn't have specified datatype ", Error: true);
                    withoutErrors = false;
                }
            }

            /* Pim */
            foreach (PimDefenition pim in elem.PerInstanceMemoryList)
            {
                if (pim.DataTypeName.Equals(AutosarApplication.ErrorDataType))
                {
                    AppendText("Component: " + elem.Name + ", PIM : " + pim.Name + " doesn't have specified datatype ", Error: true);
                    withoutErrors = false;
                }
            }

            /* Ports */
            foreach (PortDefenition port in elem.Ports)
            {
                if (port.InterfaceName.Equals(AutosarApplication.ErrorDataType))
                {
                    AppendText("Component: " + elem.Name + ", Port : " + port.Name + " doesn't have specified datatype ", Error: true);
                    withoutErrors = false;
                }
            }

            if (withoutErrors)
            {
                AppendText(elem.Name);
            }
        }

        protected void TestCompostions()
        {
            /* Check if there are similar names */
            CheckSimilarNames(autosarApp.Compositions.ConvertAll(x => x as IGUID), "Compositions");

            /* Check each enum */
            foreach (CompositionInstance elem in autosarApp.Compositions)
            {
                TestComposition(elem);
            }
        }

        protected void TestComposition(CompositionInstance elem)
        {
            bool withoutErrors = true;

            /* Check if there is an fields with similar names */
            withoutErrors &= CheckSimilarNames(elem.Ports.ConvertAll(x => x as IGUID), elem.Name + ": ports");

            /* Ports */
            foreach (PortDefenition port in elem.PortsDefenitions)
            {
                if (port.InterfaceName.Equals(AutosarApplication.ErrorDataType))
                {
                    AppendText("Composition: " + elem.Name + ", Port : " + port.Name + " doesn't have specified datatype ", Error: true);
                    withoutErrors = false;
                }
            }

            if (withoutErrors)
            {
                AppendText(elem.Name);
            }
        }

        protected void TestComponentInstances()
        {
            List<IGUID> list = new List<IGUID>();
            foreach(CompositionInstance composition in autosarApp.Compositions)
            {
                foreach(ComponentInstance component in composition.ComponentInstances)
                {
                    list.Add(component);
                }
            }

            /* Check if there are similar names */
            CheckSimilarNames(list, "component");
        }

        protected void TestErrorNames()
        {
            /* Check if there is errors with similar names */
            CheckSimilarNames(autosarApp.SystemErrors.ConvertAll(x => x as IGUID), " System errors");

            /* Check errors with similar indexes */
            for (int i = 0; i < autosarApp.SystemErrors.Count - 1; i++)
            {
                for (int j = i + 1; j < autosarApp.SystemErrors.Count; j++)
                {
                    if (autosarApp.SystemErrors[i].Value == autosarApp.SystemErrors[j].Value)
                    {
                        AppendText("There is errors with similar ID. " + autosarApp.SystemErrors[i].Name + " and " +  autosarApp.SystemErrors[j].Name + " value: " + autosarApp.SystemErrors[i].Value.ToString(), Error: true);
                    }                    
                }
            }
        }

        protected void TestOsTasks()
        {
            OsTask initTask = autosarApp.OsTasks.GetInitTask();
            for (int i = 0; i < autosarApp.OsTasks.Count; i++)
            {
                OsTask task = autosarApp.OsTasks[i];
                if (task.PeriodMs == 0)
                {
                    if (initTask != task)
                    {
                        AppendText("OsTask : " + task.Name + " has zero frequency", Error: true);
                    }
                }
            }
        }

        void AppendText(string text, bool Error = false)
        {
            if (Error)
            {
                writer.Write(ERROR);
            }
            else
            {
                writer.Write(OK);
            }
            writer.Write(text);

            writer.Write(Environment.NewLine);            
        }

        public bool CheckSimilarNames(List<IGUID> list, String datatypeName)
        {
            bool result = true;
            for (int i = 0; i < list.Count - 1; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (list[i].Name == list[j].Name)
                    {
                        AppendText("There is similar " + datatypeName + " names (" + list[i].Name + ") ", Error: true);
                        result = false;
                    }
                }
            }
            return result;
        }

        public Boolean IsErrorExist(String result)
        {
            if (result.Contains(ERROR))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
