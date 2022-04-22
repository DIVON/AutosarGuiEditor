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
                        RteGenerator rteGenerator = new RteGenerator();
                        bool result = rteGenerator.Generate();
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
            }
        }
    }
}
