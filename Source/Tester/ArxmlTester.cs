﻿using AutosarGuiEditor.Source.Autosar.Events;
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
using AutosarGuiEditor.Source.RteGenerator.CLang;
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
    enum MessageType
    {
        OK,
        WARNING,
        ERROR
    };

    public class ArxmlTester
    {
        private AutosarApplication autosarApp;
        private StringWriter outputWriter;
        private StringWriter writer;

        public ArxmlTester(AutosarApplication autosarApp, StringWriter writer)
        {
            this.autosarApp = autosarApp;
            this.outputWriter = writer;
        }

        public void Test()
        {
            writer = new StringWriter();

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
            TestQueuedSenderReceiverInterface();
            TestWetherComponentDefenitionUsed();
            TestTimingEvents();
            TestServerPorts();
            TestClientServerEvents();
            TestRunnableUsing();
            TestAllRunnablesAreUsed();
            TestRunnableNames();
            TestRunnableFrequenciesInTasks();
            TestUnusedDataTypes();
            TestUsingSenderReceiverInterfaces();
            TestUsingClientServerInterfaces();
            SortText();
        }

        void TestUsingSenderReceiverInterfaces()
        {
            foreach(var srInterface in autosarApp.SenderReceiverInterfaces)
            {
                bool used = false;
                foreach (var comp in autosarApp.ComponentDefenitionsList)
                {
                    foreach (var port in comp.Ports)
                    {
                        if (port.InterfaceDatatype == srInterface)
                        {
                            used = true;
                        }
                    }
                }
                if (!used)
                {
                    AppendText("Sender-Receiver interface is not used:  " + srInterface.Name, MessageType.WARNING);
                }
            }
        }

        void TestUsingClientServerInterfaces()
        {
            foreach (var csInterface in autosarApp.ClientServerInterfaces)
            {
                bool used = false;
                foreach (var comp in autosarApp.ComponentDefenitionsList)
                {
                    foreach (var port in comp.Ports)
                    {
                        if (port.InterfaceDatatype == csInterface)
                        {
                            used = true;
                        }
                    }
                }
                if (!used)
                {
                    AppendText("Client-Server interface is not used:  " + csInterface.Name, MessageType.WARNING);
                }
            }
        }


            void TestUnusedDataTypes()
        {
            /* Check that each client -server event has filled runnable and source */
            foreach (SimpleDataType sdt in autosarApp.SimpleDataTypes)
            {
                bool used = false;

                /* Searching in complex datatypes */
                foreach (ComplexDataType cdt in autosarApp.ComplexDataTypes)
                {
                    foreach (ComplexDataTypeField field in cdt.Fields)
                    {
                        if (field.DataTypeGUID.Equals(sdt.GUID))
                        {
                            used = true;
                        }
                    }
                }

                /* Searching in client-server operations*/
                foreach(ClientServerInterface csi in autosarApp.ClientServerInterfaces)
                {
                    foreach(ClientServerOperation csiop in csi.Operations)
                    {
                        foreach(ClientServerOperationField field in csiop.Fields)
                        {
                            if (field.BaseDataTypeGUID.Equals(sdt.GUID))
                            {
                                used = true;
                            }
                        }
                    }
                }

                /* Searching in sender-receiver interfaces */
                foreach(SenderReceiverInterface sri in autosarApp.SenderReceiverInterfaces)
                {
                    foreach (SenderReceiverInterfaceField field in sri.Fields)
                    {
                        if (field.BaseDataTypeGUID.Equals(sdt.GUID))
                        {
                            used = true;
                        }
                    }
                }

                foreach(ArrayDataType arr in autosarApp.ArrayDataTypes)
                {
                    if (arr.DataTypeGUID.Equals(sdt.GUID))
                    {
                        used = true;
                    }
                }

                if (used == false)
                {
                    AppendText("Simple datatype is not used:  " + sdt.Name, MessageType.WARNING);
                }
            }


            /* Check that each client -server event has filled runnable and source */
            foreach (ComplexDataType cdt in autosarApp.ComplexDataTypes)
            {
                bool used = false;

                /* Searching in complex datatypes */
                foreach (ComplexDataType fcdt in autosarApp.ComplexDataTypes)
                {
                    foreach (ComplexDataTypeField field in fcdt.Fields)
                    {
                        if (field.DataTypeGUID.Equals(cdt.GUID))
                        {
                            used = true;
                        }
                    }
                }

                /* Searching in client-server operations*/
                foreach (ClientServerInterface csi in autosarApp.ClientServerInterfaces)
                {
                    foreach (ClientServerOperation csiop in csi.Operations)
                    {
                        foreach (ClientServerOperationField field in csiop.Fields)
                        {
                            if (field.BaseDataTypeGUID.Equals(cdt.GUID))
                            {
                                used = true;
                            }
                        }
                    }
                }

                foreach (ArrayDataType arr in autosarApp.ArrayDataTypes)
                {
                    if (arr.DataTypeGUID.Equals(cdt.GUID))
                    {
                        used = true;
                    }
                }

                /* Searching in sender-receiver interfaces */
                foreach (SenderReceiverInterface sri in autosarApp.SenderReceiverInterfaces)
                {
                    foreach (SenderReceiverInterfaceField field in sri.Fields)
                    {
                        if (field.BaseDataTypeGUID.Equals(cdt.GUID))
                        {
                            used = true;
                        }
                    }
                }

                if (used == false)
                {
                    AppendText("Complex datatype is not used:  " + cdt.Name, MessageType.WARNING);
                }
            }
        }

        void SortText()
        {
            String[] lines = writer.ToString().Split('\n');
            foreach(String str in lines)
            {
                if (str.Contains(ERROR_STR))
                {
                    outputWriter.Write(str);
                }
            }
            foreach (String str in lines)
            {
                if (str.Contains(WARNING_STR))
                {
                    outputWriter.Write(str);
                }
            }
            foreach (String str in lines)
            {
                if ((!str.Contains(ERROR_STR)) && (!str.Contains(WARNING_STR)))
                {
                    outputWriter.Write(str);
                }
            }
        }

        const string WARNING_STR = "WARNING: ";
        const string ERROR_STR = "ERROR: ";

        void TestConnections()
        {
            foreach (CompositionInstance composition in autosarApp.Compositions)
            {
                foreach (PortConnection connection in composition.Connections)
                {
                    if (!connection.Port1.PortDefenition.InterfaceGUID.Equals(connection.Port2.PortDefenition.InterfaceGUID))
                    {
                        
                        //AppendText("Different datatypes in connection: " + connection.Component1.Name + "(" + connection.Port1.Name + ")        " + connection.Component2.Name + "(" + connection.Port2.Name + ")", MessageType.ERROR);
                        AppendText("Different datatypes in " + composition.Name + " connection: " + connection.Name, MessageType.ERROR);
                    }
                }                
            }
        }

        void TestRunnableFrequenciesInTasks()
        {
            foreach (OsTask osTask in AutosarApplication.GetInstance().OsTasks)
            {
                if (osTask.Events.Count > 0)
                {
                    for (int eventIndex = 0; eventIndex < osTask.Events.Count; eventIndex++)
                    {

                        AutosarEvent autosarEventDefenition = osTask.Events[eventIndex].Defenition;
                        if (autosarEventDefenition is TimingEvent)
                        {
                            TimingEvent timingEvent = autosarEventDefenition as TimingEvent;
                            if ((timingEvent.PeriodMs < osTask.PeriodMs) || (timingEvent.PeriodMs % osTask.PeriodMs != 0))
                            {
                                ApplicationSwComponentType compInst = AutosarApplication.GetInstance().FindComponentDefenitionByRunnnableGuid(timingEvent.Runnable.GUID);
                                AppendText("Incorrect period for timing event: " + compInst.Name + " " +autosarEventDefenition.Name, MessageType.ERROR);
                            }
                        }
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
                        AppendText("Arrays couldn't have other array datatype: " + arrDt.Name, MessageType.ERROR); 
                    }
                }
            }

            foreach (ArrayDataType arrDt in autosarApp.ArrayDataTypes)
            {
                if (arrDt.DataTypeName.Equals("ERROR"))
                {
                    AppendText("Array " + arrDt.Name + " haven't datatype", MessageType.ERROR);
                }
            }

            foreach (ArrayDataType arrDt in autosarApp.ArrayDataTypes)
            {
                if (arrDt.Size <= 0)
                {
                    AppendText("Array " + arrDt.Name + " have zero size!", MessageType.ERROR);
                }
            }
        }

        void TestForComplexDependency()
        {
            ComplexDataTypesList allCDT = new ComplexDataTypesList();
            allCDT.Capacity = autosarApp.ComplexDataTypes.Count;
            allCDT.AddRange(autosarApp.ComplexDataTypes);
            RteDataTypesGenerator_C.RemoveDataTypesWithoutDependencies(allCDT);
            bool result = RteDataTypesGenerator_C.SortDependenciedCDT(allCDT);
            if (!result)
            {
                String strRes = "There are recursion dependencies in components: " + Environment.NewLine;
                foreach (ComplexDataType cdt in allCDT)
                {
                    strRes += cdt.Name + Environment.NewLine;
                }
                AppendText(strRes, MessageType.ERROR);                
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
            for (int i = 0; i < enumDt.Fields.Count - 1; i++)
            {
                for (int j = i + 1; j < enumDt.Fields.Count; j++)
                {
                    if (enumDt.Fields[i].Value == enumDt.Fields[j].Value)
                    {
                        AppendText(enumDt.Name + " field value " + enumDt.Fields[i].Name + "equals to " + enumDt.Fields[j].Name + " (" + enumDt.Fields[i].Value + ")", MessageType.ERROR);
                    }
                    if (enumDt.Fields[i].Name == enumDt.Fields[j].Name)
                    {
                        AppendText(enumDt.Name + " has similar names (" + enumDt.Fields[i].Name + ") of fields for " + i.ToString() + " and " + j.ToString() + " indexes", MessageType.ERROR);
                    }
                }
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
            /* Check if fields have similar names */
            for (int i = 0; i < elem.Fields.Count - 1; i++)
            {
                for (int j = i + 1; j < elem.Fields.Count; j++)
                {
                    if (elem.Fields[i].Name == elem.Fields[j].Name)
                    {
                        AppendText(elem.Name + " has similar field names (" + elem.Fields[i].Name + ") of fields for " + i.ToString() + " and " + j.ToString() + " indexes", MessageType.ERROR);
                    }
                }
            }

            /* CheckDatatype */
            foreach (ComplexDataTypeField field in elem.Fields)
            {
                if (field.DataTypeName.Equals(AutosarApplication.ErrorDataType))
                {
                    AppendText(elem.Name + " : " + field.Name + " doesn't have specified datatype ", MessageType.ERROR);
                }
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
                AppendText(elem.Name + " :  does not have any operations!", MessageType.ERROR);
            }

            /* Check Asyncronous */
            if (elem.IsAsync == true)
            {
                foreach (ClientServerOperation operation in elem.Operations)
                {
                    if (operation.Fields.Count > 0)
                    {
                        AppendText(elem.Name + " : " + operation.Name + " : Async calls shall have no arguments ", MessageType.ERROR);
                    }
                }
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
                        AppendText(elem.Name + " : " + operation.Name + " : " + field.Name + " doesn't have specified datatype ", MessageType.ERROR);
                         withoutErrors = false;
                    }
                }                
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
                AppendText(elem.Name + " :  does not have any fields!", MessageType.ERROR);
            }

            
            /* Check if there is an fields with similar names */
            withoutErrors &= CheckSimilarNames(elem.Fields.ConvertAll(x => x as IGUID), elem.Name + ": fields");

            /* Check datatypes */

            foreach (SenderReceiverInterfaceField field in elem.Fields)
            {
                if (field.DataTypeName.Equals(AutosarApplication.ErrorDataType))
                {
                    AppendText(elem.Name + " : " + field.Name + " doesn't have specified datatype ", MessageType.ERROR);
                    withoutErrors = false;
                }
            }
        }

        protected void TestComponentDefenition()
        {
            /* Check if there are similar names */
            CheckSimilarNames(autosarApp.ComponentDefenitionsList.ConvertAll(x => x as IGUID), "Component Defenition");

            /* Check each enum */
            foreach (ApplicationSwComponentType elem in autosarApp.ComponentDefenitionsList)
            {
                TestComponentDefenition(elem);
            }
        }

        protected void TestComponentDefenition(ApplicationSwComponentType elem)
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
                    AppendText("Component: " + elem.Name + ", Cdata : " + cdata.Name + " doesn't have specified datatype ", MessageType.ERROR);
                    withoutErrors = false;
                }
            }

            /* Pim */
            foreach (PimDefenition pim in elem.PerInstanceMemoryList)
            {
                if (pim.DataTypeName.Equals(AutosarApplication.ErrorDataType))
                {
                    AppendText("Component: " + elem.Name + ", PIM : " + pim.Name + " doesn't have specified datatype ", MessageType.ERROR);
                    withoutErrors = false;
                }
            }

            /* Ports */
            foreach (PortDefenition port in elem.Ports)
            {
                if (port.InterfaceName.Equals(AutosarApplication.ErrorDataType))
                {
                    AppendText("Component: " + elem.Name + ", Port : " + port.Name + " doesn't have specified datatype ", MessageType.ERROR);
                    withoutErrors = false;
                }
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
                    AppendText("Composition: " + elem.Name + ", Port : " + port.Name + " doesn't have specified datatype ", MessageType.ERROR);
                    withoutErrors = false;
                }
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

        protected void TestWetherComponentDefenitionUsed()
        {
            foreach (ApplicationSwComponentType compDef in autosarApp.ComponentDefenitionsList)
            {
                Boolean used = false;
                foreach (CompositionInstance composition in autosarApp.Compositions)
                {
                    foreach (ComponentInstance compInstance in composition.ComponentInstances)
                    {
                        if (compInstance.ComponentDefenition == compDef)
                        {
                            used = true;
                            break;                            
                        }
                    }
                    if (used)
                    {
                        break;
                    }
                }
                if (!used)
                {
                    AppendText("Component defenition : " + compDef.Name + " is not used", MessageType.WARNING);
                }
            }
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
                    //if (autosarApp.SystemErrors[i].Value == autosarApp.SystemErrors[j].Value)
                    //{
                    //    AppendText("There is errors with similar ID. " + autosarApp.SystemErrors[i].Name + " and " +  autosarApp.SystemErrors[j].Name + " value: " + autosarApp.SystemErrors[i].Value.ToString(), MessageType.ERROR);
                    //}                    
                }
            }
        }

        protected void TestOsTasks()
        {
#if false
            OsTask initTask = autosarApp.OsTasks.GetInitTask();
            for (int i = 0; i < autosarApp.OsTasks.Count; i++)
            {
                OsTask task = autosarApp.OsTasks[i];
                if (task.PeriodMs == 0)
                {
                    if (initTask != task)
                    {
                        AppendText("OsTask : " + task.Name + " has zero frequency", MessageType.ERROR);
                    }
                }
            }
#endif
        }

        protected void TestTimingEvents()
        {
            foreach (ApplicationSwComponentType compDef in autosarApp.ComponentDefenitionsList)
            {
                foreach(TimingEvent timingEvent in compDef.TimingEvents)
                {
                    foreach (TimingEvent timingEvent2 in compDef.TimingEvents)
                    {
                        if (timingEvent2 != timingEvent)
                        {
                            if (timingEvent.Name.Equals(timingEvent2.Name))
                            {
                                AppendText("Component shall have no similar timing events : " + timingEvent.Name, MessageType.ERROR);
                            }
                        }
                    }
                }
            }

            /*  Check that period is more than zero */
            foreach (ApplicationSwComponentType compDef in autosarApp.ComponentDefenitionsList)
            {
                foreach (TimingEvent timingEvent in compDef.TimingEvents)
                {
                    if (timingEvent.PeriodMs <= 0)
                    {
                        AppendText("Timing event shall have period more than zero: " + compDef.Name + " " + timingEvent.Name, MessageType.ERROR);
                    }
                }
            }
        }

        void TestClientServerEvents()
        {
            /* Check that each client -server event has filled runnable and source */
            foreach (ApplicationSwComponentType compDef in autosarApp.ComponentDefenitionsList)
            {
                foreach (ClientServerEvent csEvent in compDef.SyncClientServerEvents)
                {
                    if (csEvent.Runnable == null)
                    {
                        AppendText("Each sync client-server events shall have filled runnables : " + compDef.Name + " event: " + csEvent.Name, MessageType.ERROR);
                    }
                    if (csEvent.SourceOperation == null)
                    {
                        AppendText("Each sync client-server events shall have filled source operation : " + compDef.Name + " event: " + csEvent.Name, MessageType.ERROR);
                    }
                }

                foreach (ClientServerEvent csEvent in compDef.AsyncClientServerEvents)
                {
                    if (csEvent.Runnable == null)
                    {
                        AppendText("Each async client-server events shall have filled runnables : " + compDef.Name + " event: " + csEvent.Name, MessageType.ERROR);
                    }
                    if (csEvent.SourceOperation == null)
                    {
                        AppendText("Each async client-server events shall have filled source operation : " + compDef.Name + " event: " + csEvent.Name, MessageType.ERROR);
                    }
                }
            }
        }

        void TestRunnableUsing()
        {
            /* Check that each client -server event has filled runnable and source */
            foreach (ApplicationSwComponentType compDef in autosarApp.ComponentDefenitionsList)
            {
                foreach (RunnableDefenition runnableDef in compDef.Runnables)
                {
                    Boolean runnableUsedInPeriodicEvents = false;
                    Boolean runnableUsedInClientServerEvents = false;

                    foreach (ClientServerEvent csEvent in compDef.SyncClientServerEvents)
                    {
                        if (csEvent.Runnable != null)
                        {
                            if (csEvent.Runnable == runnableDef)
                            {
                                runnableUsedInClientServerEvents = true;
                            }
                        }
                    }

                    foreach (ClientServerEvent csEvent in compDef.AsyncClientServerEvents)
                    {
                        if (csEvent.Runnable != null)
                        {
                            if (csEvent.Runnable == runnableDef)
                            {
                                runnableUsedInClientServerEvents = true;
                            }
                        }
                    }

                    foreach (TimingEvent timingEvent in compDef.TimingEvents)
                    {
                        if (timingEvent.Runnable != null)
                        {
                            if (timingEvent.Runnable == runnableDef)
                            {
                                runnableUsedInPeriodicEvents = true;
                            }
                        }
                    }


                    foreach (OneTimeEvent oneTimeEvent in compDef.OneTimeEvents)
                    {
                        if (oneTimeEvent.Runnable != null)
                        {
                            if (oneTimeEvent.Runnable == runnableDef)
                            {
                                runnableUsedInPeriodicEvents = true;
                            }
                        }
                    }

                    if (runnableUsedInPeriodicEvents && runnableUsedInClientServerEvents)
                    {
                        AppendText("The same runnable shall not be used for periodical and client-server events : " + compDef.Name + " runnable: " + runnableDef.Name, MessageType.ERROR);
                    }
                }
            }
        }

        void TestRunnableNames()
        {
            /* Check that each client -server event has filled runnable and source */
            foreach (ApplicationSwComponentType compDef in autosarApp.ComponentDefenitionsList)
            {
                foreach (RunnableDefenition runnableDef in compDef.Runnables)
                {
                    if (runnableDef.Name.Length < 2)
                    {
                        AppendText("The runnable has incorrect name : " + compDef.Name + " runnable: " + runnableDef.Name, MessageType.ERROR);
                    }
                }
            }
        }

        void TestAllRunnablesAreUsed()
        {
            /* Check that each client -server event has filled runnable and source */
            foreach (ApplicationSwComponentType compDef in autosarApp.ComponentDefenitionsList)
            {
                foreach (RunnableDefenition runnableDef in compDef.Runnables)
                {
                    Boolean runnableUsed = false;

                    foreach (ClientServerEvent csEvent in compDef.SyncClientServerEvents)
                    {
                        if (csEvent.Runnable != null)
                        {
                            if (csEvent.Runnable == runnableDef)
                            {
                                runnableUsed = true;
                            }
                        }
                    }

                    foreach (ClientServerEvent csEvent in compDef.AsyncClientServerEvents)
                    {
                        if (csEvent.Runnable != null)
                        {
                            if (csEvent.Runnable == runnableDef)
                            {
                                runnableUsed = true;
                            }
                        }
                    }

                    foreach (TimingEvent timingEvent in compDef.TimingEvents)
                    {
                        if (timingEvent.Runnable != null)
                        {
                            if (timingEvent.Runnable == runnableDef)
                            {
                                runnableUsed = true;
                            }
                        }
                    }


                    foreach (OneTimeEvent oneTimeEvent in compDef.OneTimeEvents)
                    {
                        if (oneTimeEvent.Runnable != null)
                        {
                            if (oneTimeEvent.Runnable == runnableDef)
                            {
                                runnableUsed = true;
                            }
                        }
                    }

                    if (runnableUsed == false)
                    {
                        AppendText("The runnable is not used : " + compDef.Name + " runnable: " + runnableDef.Name, MessageType.WARNING);
                    }
                }
            }
        }

        protected void TestQueuedSenderReceiverInterface()
        {
            /* Queued Sender-Receiver interfaces shall have only one field  */
            foreach (SenderReceiverInterface srInterface in autosarApp.SenderReceiverInterfaces)
            {
                if (srInterface.IsQueued)
                {
                    if (srInterface.Fields.Count > 1u)
                    {
                        //AppendText("Queued Sender-Receiver interface shall have only one field : " + srInterface.Name, MessageType.ERROR);
                    }
                }
            }
        }

        protected void TestServerPorts()
        {
            foreach (ApplicationSwComponentType compDef in autosarApp.ComponentDefenitionsList)
            {
                foreach (PortDefenition portDef in compDef.Ports)
                {
                    if (portDef.PortType == PortType.Server)
                    {
                        if (portDef.InterfaceDatatype is ClientServerInterface)
                        {
                            ClientServerInterface csInterface = portDef.InterfaceDatatype as ClientServerInterface;
                            foreach (ClientServerOperation csOperation in csInterface.Operations)
                            {
                                /* Looking for runnable of operation */
                                ClientServerEvent csEvent = compDef.GetEventsWithServerOperation(csOperation);
                                if (csEvent == null)
                                {
                                    AppendText("Each operation at server port shall have event: " + compDef.Name + " " + portDef.Name + " " + csOperation.Name, MessageType.ERROR);
                                }
                            }
                        }
                    }
                }
            }
        }

        void AppendText(string text, MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.OK:
                {
                    break;
                }
                case MessageType.WARNING:
                {
                    writer.Write(WARNING_STR);
                    break;
                }
                case MessageType.ERROR:
                {
                    writer.Write(ERROR_STR);
                    break;
                }
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
                        AppendText("There is similar " + datatypeName + " names (" + list[i].Name + ") ", MessageType.ERROR);
                        result = false;
                    }
                }
            }
            return result;
        }

        public Boolean IsErrorExist(String result)
        {
            if (result.Contains(ERROR_STR))
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
