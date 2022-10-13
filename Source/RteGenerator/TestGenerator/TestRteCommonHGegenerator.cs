using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Composition;
using AutosarGuiEditor.Source.Painters;
using System;
using System.IO;
using AutosarGuiEditor.Source.RteGenerator.CLang;

namespace AutosarGuiEditor.Source.RteGenerator.TestGenerator
{
    public class TestRteCommonHGegenerator
    {
        public void GenerateRteTestCommonHFile(String folder)
        {
            String filename = folder + "\\" + "Rte_TestCommon.h";
            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator_C.GenerateFileTitle(writer, filename, "Implementation for common test data");

            RteFunctionsGenerator_C.OpenGuardDefine(writer);
            writer.WriteLine("");

            RteFunctionsGenerator_C.OpenCGuardDefine(writer);

            /*Add #include */
            RteFunctionsGenerator_C.AddInclude(writer, "<string.h>");
            RteFunctionsGenerator_C.AddInclude(writer, Properties.Resources.RTE_DATATYPES_H_FILENAME);
            AddComponentIncludes(writer);


            GenerateTestStubRecordType(writer);
            RteConnectionGenerator_C.GenerateExternComponentInstances(writer);

            RteFunctionsGenerator_C.CloseCGuardDefine(writer);
            RteFunctionsGenerator_C.CloseGuardDefine(writer);
            writer.Close();
        }

        void AddComponentIncludes(StreamWriter writer)
        {
            foreach (ApplicationSwComponentType compDef in AutosarApplication.GetInstance().ComponentDefenitionsList)
            {
                RteFunctionsGenerator_C.AddInclude(writer, "<" + compDef.Name + "_TestRte.h>");
            }
            writer.WriteLine("");
        }

        void GenerateTestStubRecordType(StreamWriter writer)
        {
            writer.WriteLine("typedef struct TEST_STUB_RECORD_TYPE");
            writer.WriteLine("{");
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    if (component.ComponentDefenition.IsComponentGenerable())
                    {
                        ApplicationSwComponentType compDef = component.ComponentDefenition;
                        String compDataType = TestRteEnvironmentGenerator.TestArtefactsStructureDataType(compDef);
                        writer.WriteLine("    " + compDataType + " " + component.Name + ";");
                    }
                    else
                    {
                        writer.WriteLine("/* " + component.Name + " instance is empty. Nothing to generate. */");
                    }
                }
            }
            writer.WriteLine("} TEST_STUB_RECORD_TYPE;");
            writer.WriteLine("");
            writer.WriteLine("extern TEST_STUB_RECORD_TYPE TEST_STUB_RECORD;");
            writer.WriteLine("");
        }
    }
}
