using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalMaxscript.Sender;

namespace CommandLineApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null)
            {
                throw new NullReferenceException("Parameter [args] can't be null or empty");
            }

            bool result = ExternalMaxscriptSender.Initialize();


            if (result)
            {
                foreach (var s in args)
                {
                //    if (File.Exists(args[0]))
                //        ExternalMaxscriptSender.EvaluateMaxScript(s);
                //    else
                //        ExternalMaxscriptSender.ExecuteMAXScriptScript(s);

                    ExternalMaxscriptSender.Log("test");
                }

            }
            else
            {
                throw new Exception("Unable to find External Maxscript Listener window");
            }
        }
    }
}
