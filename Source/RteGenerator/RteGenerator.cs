using System;
using System.Resources;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.DataTypes;
using AutosarGuiEditor.Source.DataTypes.Enum;
using AutosarGuiEditor.Source.DataTypes.BaseDataType;
using AutosarGuiEditor.Source.DataTypes.ComplexDataType;
using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;
using AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver;
using AutosarGuiEditor.Source.Painters.PortsPainters;
using AutosarGuiEditor.Source.Painters;
using AutosarGuiEditor.Source.PortDefenitions;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Composition;
using AutosarGuiEditor.Source.Render;
using AutosarGuiEditor.Source.Painters.Components.PerInstance;
using AutosarGuiEditor.Source.Component.PerInstanceMemory;
using AutosarGuiEditor.Source.SystemInterfaces;
using AutosarGuiEditor.Source.DataTypes.ArrayDataType;
using AutosarGuiEditor.Source.Component.CData;
using AutosarGuiEditor.Source.Painters.Components.CData;

namespace AutosarGuiEditor.Source.RteGenerator
{
    public class RteGenerator
    {
        public bool Generate()
        {
            /* Create base folders */
            Directory.CreateDirectory(RteFunctionsGenerator.GetRteFolder());
            GenerateDataTypesFile(RteFunctionsGenerator.GetRteFolder());
            GenerateScheduler();

            RteConnectionCGenerator connectionsGenerator = new RteConnectionCGenerator();
            connectionsGenerator.GenerateConnections(RteFunctionsGenerator.GetRteFolder());

            ReturnCodesGenerator returnCodesGenerator = new ReturnCodesGenerator();
            returnCodesGenerator.GenerateReturnCodesFile(RteFunctionsGenerator.GetRteFolder());

            /* Create system errors file */
            SystemErrorGenerator systemErrorGenerator = new SystemErrorGenerator();
            systemErrorGenerator.GenerateSystemErrorsFile(RteFunctionsGenerator.GetRteFolder());

            return true;
        }

       
        void GenerateDataTypesFile(String folder)
        {
            RteDataTypesGenerator dataTypesGenerator = new RteDataTypesGenerator();
            dataTypesGenerator.GenerateDataTypesFile(folder);
        }

        
        public void GenerateComponentsFiles()
        {
            Directory.CreateDirectory(RteFunctionsGenerator.GetComponentsFolder());

            RteComponentGenerator componentGenerator = new RteComponentGenerator();
            componentGenerator.GenerateComponentsFiles();
        }
        

        void GenerateScheduler()
        {
            RteSchedulerGenerator schedulerGenerator = new RteSchedulerGenerator();
            schedulerGenerator.GenerateShedulerFiles(RteFunctionsGenerator.GetRteFolder());
        }
    }
}
