using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Composition;
using AutosarGuiEditor.Source.Painters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.RteGenerator.TestGenerator
{
    public class TestRteCommonHGegenerator
    {
        public void GenerateRteTestCommonHFile(String folder)
        {
            String filename = folder + "\\" + "Rte_TestCommon.h";
            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator.GenerateFileTitle(writer, filename, "Implementation for common test data");

            RteFunctionsGenerator.OpenGuardDefine(writer);
            writer.WriteLine("");

            /*Add #include */
            RteFunctionsGenerator.AddInclude(writer, "<string.h>");
            RteFunctionsGenerator.AddInclude(writer, Properties.Resources.RTE_DATATYPES_H_FILENAME);
            AddComponentIncludes(writer);


            GenerateTestStubRecordType(writer);
            GenerateExternComponentInstances(writer);

            RteFunctionsGenerator.CloseGuardDefine(writer);
            writer.Close();
        }

        void AddComponentIncludes(StreamWriter writer)
        {
            foreach (ApplicationSwComponentType compDef in AutosarApplication.GetInstance().ComponentDefenitionsList)
            {
                RteFunctionsGenerator.AddInclude(writer, "<" + compDef.Name + "_TestRte.h>");
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
                    ApplicationSwComponentType compDef = component.ComponentDefenition;
                    String compDataType = TestRteEnvironmentGenerator.TestArtefactsStructureDataType(compDef);
                    writer.WriteLine("    " + compDataType + " " + component.Name + ";");
                }
            }
            writer.WriteLine("} TEST_STUB_RECORD_TYPE;");
            writer.WriteLine("");
            writer.WriteLine("extern TEST_STUB_RECORD_TYPE TEST_STUB_RECORD;");
            writer.WriteLine("");
        }

        void GenerateExternComponentInstances(StreamWriter writer)
        {
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;
                    String CDSname = RteFunctionsGenerator.ComponentDataStructureDefenitionName(compDef);
                    writer.WriteLine("extern const " + CDSname + " Rte_Instance_" + component.Name + ";");                    
                }
            }
            writer.WriteLine("");
        }
    }
}
