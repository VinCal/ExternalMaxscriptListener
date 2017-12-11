using System;
using System.IO;
using ExternalMaxscript.Sender;

namespace Test_Application
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            bool result = ExternalMaxscriptSender.Initialize();

            if (result)
            {
                Console.WriteLine("Type: \"exit\" to exit this application.");
                Console.WriteLine("Enter Maxscript commands:");

                while (true)
                {
                    string line = Console.ReadLine(); 
                    if (line == "exit")
                        Environment.Exit(0);
                    
                    if (File.Exists(line))
                        ExternalMaxscriptSender.EvaluateMaxScript(line);
                    else
                    {
                        // We can type: "Test -log" to print "Test" to the MAXScript Listener log.
                        int typeIndex = line.LastIndexOf('-');

                        string substring = line.Substring(typeIndex + 1);
                        if (string.Equals(substring, "log", StringComparison.OrdinalIgnoreCase))
                        {
                            ExternalMaxscriptSender.Log(line.Substring(0, typeIndex));
                        }
                        else
                        {
                            ExternalMaxscriptSender.ExecuteMaxScriptScript(line);
                        }
                    }
                }
            }

            Console.WriteLine("Unable to find External Maxscript Listener window");
            Console.ReadLine();
        }
    }
}
