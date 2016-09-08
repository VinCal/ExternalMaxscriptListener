using System;
using System.Diagnostics;
using System.IO;
using ExternalMaxscript;

namespace Test_Application
{
    class Program
    {
        static void Main(string[] args)
        {
            bool result = ExternalMaxscriptSender.Initialize();

            if (result)
            {
                Console.WriteLine(@"Type: ""exit"" to exit this application.");
                Console.WriteLine("Enter Maxscript commands:");

                while (true)
                {
                    string line = Console.ReadLine(); // Get string from user
                    if (line == "exit") // Check string
                        Environment.Exit(0);

                    //if (File.Exists(line))
                    //    ExternalMaxscriptSender.EvaluateMaxScript(line);
                    //else
                    //    ExternalMaxscriptSender.ExecuteMAXScriptScript(line);

                    ExternalMaxscriptSender.Log(line);
                }
            }
            else
            {
                Console.WriteLine("Unable to find External Maxscript Listener window");
            }

            Console.ReadLine();

            //Process proc = Process.Start(@"C:\Users\Vincent\Documents\GitHub\ExternalMaxscriptListener\CommandLineApp\bin\x64\Release\CommandLineApp.exe", "Box()");
        }
    }
}
