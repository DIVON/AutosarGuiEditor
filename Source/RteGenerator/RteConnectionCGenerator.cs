using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Component.PerInstanceMemory;
using AutosarGuiEditor.Source.Composition;
using AutosarGuiEditor.Source.Painters;
using AutosarGuiEditor.Source.Painters.Components.CData;
using AutosarGuiEditor.Source.Painters.Components.PerInstance;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.RteGenerator
{
    public class RteConnectionCGenerator
    {
        public void GenerateConnections()
        {
            String filename = RteFunctionsGenerator.GetRteFolder() + "\\" + Properties.Resources.RTE_CONNECTIONS_C_FILENAME;
            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator.GenerateFileTitle(writer, filename, "Implementation for RTE connections source file");

            /*Add #include */
            RteFunctionsGenerator.AddInclude(writer, "<string.h>");
            RteFunctionsGenerator.AddInclude(writer, Properties.Resources.RTE_DATATYPES_H_FILENAME);
            RteFunctionsGenerator.AddInclude(writer, Properties.Resources.SYSTEM_ERRORS_H_FILENAME);
            RteFunctionsGenerator.AddInclude(writer, Properties.Resources.RTE_EXTERNAL_RUNNABLES_H_FILENAME);

            /* Include */
            writer.WriteLine("");

            GenerateAllPimBuffers(writer);
            GenerateAllCDataBuffers(writer);

            
            GenerateAllComponentInstances(writer);

            /* Generate buffers for all write ports and queued read ports */


            /* Generate all rte write functions */
            foreach (ApplicationSwComponentType compDef in AutosarApplication.GetInstance().ComponentDefenitionsList)
            {
                writer.WriteLine("/**************************************************");
                writer.WriteLine("        " + compDef.Name);
                writer.WriteLine("***************************************************/");

                /* Check if this components exists */
                if (AutosarApplication.GetInstance().GetComponentInstanceByDefenition(compDef).Count > 0)
                {
                 //   CreateRteWriteFunctions(writer, compDef);

                 //   CreateRteReadFunctions(writer, compDef);

                //    CreateRteCallFunctions(writer, compDef);

                 //   CreateRtePimFunctions(writer, compDef);

                 //   CreateRteCDataFunctions(writer, compDef);
                }


            }

            writer.Close();
        }


        void GenerateAllPimBuffers(StreamWriter writer)
        {
            /* Without multiple instantiation */
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;

                    foreach (PimInstance pim in component.PerInstanceMemories)
                    {
                        String pimName = pim.Defenition.DataTypeName + " Rte_PimBuffer_" + component.Name + "_" + pim.Name;
                        String defaultValue = pim.GetDefaultValue();
                        if (defaultValue.Length > 0)
                        {
                            pimName += " = " + defaultValue + ";";
                        }
                        else
                        {
                            pimName += ";";
                        }
                        writer.WriteLine(pimName);
                    }
                }
            }
        }

        void GenerateAllCDataBuffers(StreamWriter writer)
        {
            /* Without multiple instantiation */
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;

                    foreach (CDataInstance cdata in component.CDataInstances)
                    {
                        String cdataName = "Rte_CDataBuffer_" + component.Name + "_" + cdata.Name;
                        String defValue = cdata.GetDefaultValue();
                        String writeString = "const " + cdata.Defenition.DataTypeName + " " + cdataName + " = " + defValue + ";";
                        writer.WriteLine(writeString);                        
                    }
                }
            }
        }


        void CreateRtePimBuffer(StreamWriter writer, ApplicationSwComponentType compDefenition)
        {
            foreach (PimDefenition pim in compDefenition.PerInstanceMemoryList)
            {
                GenerateRtePimFunction(writer, compDefenition, pim);
            }
        }

        void GenerateRtePimFunction(StreamWriter writer, ApplicationSwComponentType compDef, PimDefenition pimDef)
        {
            String returnValue = pimDef.DataTypeName + " * const ";
            String RteFuncName = RteFunctionsGenerator.GenerateFullPimFunctionName(compDef, pimDef);

            String fieldVariable = RteFunctionsGenerator.GenerateMultipleInstanceFunctionArguments(compDef.MultipleInstantiation);

            writer.WriteLine(returnValue + RteFuncName + fieldVariable);
            writer.WriteLine("{");

            if (!compDef.MultipleInstantiation) //single instantiation component 
            {
                ComponentInstancesList components = AutosarApplication.GetInstance().GetComponentInstanceByDefenition(compDef);
                if (components.Count > 0)
                {
                    /* At least one component exists at this step */
                    ComponentInstance compInstance = components[0];
                    String compName = RteFunctionsGenerator.GenerateComponentName(compInstance.Name);
                    String field = RteFunctionsGenerator.GenerateRtePimFieldInComponentDefenitionStruct(compDef, pimDef);
                    writer.WriteLine("    return &" + compName + "." + field + ";");
                }
            }
            else //multiple instantiation component 
            {
                String field = RteFunctionsGenerator.GenerateRtePimFieldInComponentDefenitionStruct(compDef, pimDef);
                writer.WriteLine("    return &((" + compDef.Name + "*)instance)->" + field + ";");
            }

            writer.WriteLine("}");
            writer.WriteLine("");
        }


        void GenerateAllComponentInstances(StreamWriter writer)
        {
            /* Without multiple instantiation */
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    //GenerateComponentInstance(writer, component);
                }
            }
        }
    }
}
