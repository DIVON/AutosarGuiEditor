using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AutosarGuiEditor.Source.Tester;
using AutosarGuiEditor.Source.RteGenerator;
using System.Diagnostics;
using System.IO;
using AutosarGuiEditor.Source.RteGenerator.TestGenerator;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.RteGenerator.CLang;
using AutosarGuiEditor.Source.RteGenerator.CppLang;

namespace AutosarGuiEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // here you take control

            if (e.Args.Length == 2)
            {
                if (e.Args[0] == "-RteRegen")
                {
                    AutosarApplication app = AutosarApplication.GetInstance();
                    String file = e.Args[1].Replace("\"", "");
                    app.LoadFromFile(file);
                    StringWriter writer = new StringWriter();

                    ArxmlTester tester = new ArxmlTester(app, writer);
                    tester.Test();
                    String testResult = writer.ToString();
                    if (!tester.IsErrorExist(testResult))
                    {
                        bool result = false;
                        if (app.ProgramLanguage.Type == ProgrammingLanguageTypeDef.C)
                        {
                            RteGenerator_C rteGenerator = new RteGenerator_C();
                            result = rteGenerator.Generate();
                        }
                        else if (app.ProgramLanguage.Type == ProgrammingLanguageTypeDef.Cpp)
                        {
                            RteGenerator_Cpp rteGenerator = new RteGenerator_Cpp();
                            result = rteGenerator.Generate();
                        }


                        if (result == true)
                        {
                            Console.WriteLine("RTE has been generated.");
                        }
                        else
                        {
                            Console.WriteLine("There is a problem with RTE generation.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("There are errors in the project. RTE generation is impossible. Check project for errors.");
                    }

                    Environment.Exit(0);
                }

                if (e.Args[0] == "-SkeletonRegen")
                {
                    AutosarApplication app = AutosarApplication.GetInstance();
                    String file = e.Args[1].Replace("\"", "");
                    app.LoadFromFile(file);
                    StringWriter writer = new StringWriter();

                    ArxmlTester tester = new ArxmlTester(app, writer);
                    tester.Test();
                    String testResult = writer.ToString();
                    if (!tester.IsErrorExist(testResult))
                    {
                        if (app.ProgramLanguage.Type == ProgrammingLanguageTypeDef.C)
                        {
                            RteGenerator_C rteGenerator = new RteGenerator_C();
                            rteGenerator.GenerateComponentsFiles();
                        }
                        else if (app.ProgramLanguage.Type == ProgrammingLanguageTypeDef.Cpp)
                        {
                            RteGenerator_Cpp rteGenerator = new RteGenerator_Cpp();
                            rteGenerator.GenerateComponentsFiles();
                        }

                        Console.WriteLine("RTE has been generated.");
                    }
                    else
                    {
                        Console.WriteLine("There are errors in the project. RTE generation is impossible. Check project for errors.");
                    }

                    Environment.Exit(0);
                }

                if (e.Args[0] == "-TestRteRegen")
                {
                    AutosarApplication autosarApp = AutosarApplication.GetInstance();
                    String file = e.Args[1].Replace("\"", "");
                    autosarApp.LoadFromFile(file);
                    StringWriter writer = new StringWriter();

                    ArxmlTester tester = new ArxmlTester(autosarApp, writer);
                    tester.Test();
                    String testResult = writer.ToString();
                    if (!tester.IsErrorExist(testResult))
                    {
                        TestRteEnvironmentGenerator generator = new TestRteEnvironmentGenerator();
                        foreach (ApplicationSwComponentType compDefenition in AutosarApplication.GetInstance().ComponentDefenitionsList)
                        {
                            generator.GenerateRteEnvironment(compDefenition, autosarApp.GenerateTestRtePath);

                        }
                        generator.GenerateCommonFiles(autosarApp.GenerateTestRtePath);
                        RteSchedulerGenerator_C rteSchedulerGenerator = new RteSchedulerGenerator_C();
                        rteSchedulerGenerator.Generate_ExternalRunnables_File(autosarApp.GenerateTestRtePath);
                    }
                    else
                    {
                        Console.WriteLine("There are errors in the project. RTE generation is impossible. Check project for errors.");
                    }

                    Environment.Exit(0);
                }
            }
        }
    }
}
