using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Composition;
using System;
using System.IO;

namespace AutosarGuiEditor.Source.RteGenerator.CppLang
{
    public class RteObjectConstructorGenerator_Cpp
    {
        public void GenerateConstructosFile(String folder)
        {
            String filename = folder + "\\" + Properties.Resources.RTE_CONSTRUCTORS_CPP_FILENAME;
            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator_Cpp.GenerateFileTitle(writer, filename, "Contains implementation for default component's constructors");

            /* Add #include */
            RteConnectionGenerator_Cpp connectionsGenerator = new RteConnectionGenerator_Cpp();
            connectionsGenerator.AddComponentIncludesWithoutRte(writer);

            writer.WriteLine();

            foreach (ApplicationSwComponentType compDef in AutosarApplication.GetInstance().ComponentDefenitionsList)
            {
                GenerateConstructor(writer, compDef);
            }
            writer.Close();
        }

        void GenerateConstructor(StreamWriter writer, ApplicationSwComponentType compDef)
        {
            if ((compDef.Runnables.Count == 0) && (compDef.Ports.Count == 0))
            {
                return;
            }

            String rteStructureName = RteFunctionsGenerator_Cpp.ComponentRteDataStructureDefenitionName(compDef);
            String baseClassName = RteFunctionsGenerator_Cpp.ComponentBaseClassDefenitionName(compDef);

            String line = compDef.Name + "::" + compDef.Name + "(const " + rteStructureName + " &rte):" + baseClassName + "(rte) {}";
            writer.WriteLine(line);
        }
    }
}
