using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using MAXScriptParserTypes;

namespace ExternalMaxscript.Sender
{
    // TODO Add class description
    public static class ExternalMaxscriptSender
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        private static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", SetLastError = true, EntryPoint = "SendMessage")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);

        // Datamember to keep track of the Window handle for our External Maxscript Listener window. 
        private static IntPtr _ExternalMxsListenerHwnd;

        /// <summary>
        /// This methods should be called once to connect to the External Maxscript Listener plugin loaded by 3ds Max.
        /// </summary>
        /// <returns>True if connecting and sending the WM_USER_INITIALIZE message was successful, false otherwise</returns>
        [ComVisible(true)]
        public static bool Initialize()
        {
            _ExternalMxsListenerHwnd = FindWindowByCaption(IntPtr.Zero, "External Maxscript Listener");
            if (_ExternalMxsListenerHwnd == IntPtr.Zero) return false;

            //Send initialize message to check if it's our window and not a random one with the exact same name...
            IntPtr resultPtr = SendMessage(_ExternalMxsListenerHwnd, WindowMessages.WM_USER_INITIALIZE, (IntPtr)1, IntPtr.Zero);
            int initResult = resultPtr.ToInt32();

            return initResult == 1;
        }

        /// <summary>
        /// Sends msg to External Maxscript Listener, which in turn executes the maxscript code.
        /// </summary>
        /// <param name="msg">Points to a null-terminated string that specifies the MAXScript commands to compile and evaluate. 
        /// This expects a string containing Maxscript expressions, NOT a file path.</param>
        [ComVisible(true)]
        public static void ExecuteMaxScriptScript(string msg)
        {
            if (_ExternalMxsListenerHwnd == IntPtr.Zero)
            {
                throw new NullReferenceException("Can't send message, External Maxscript Listener is not found." + Environment.NewLine + @"Did you forget to Call: ""FindExternalMaxscriptListener()""?");
            }

            if (msg.Length <= 0)
            {
                throw new ArgumentException("Message string has no characters.", "msg");
            }

            SendMaxMessage(msg, MessageType.Command);
        }

        /// <summary>
        /// Send path to External Maxscript Listener, which in turn evaluates the script at the given location.
        /// </summary>
        /// <param name="path">The fully qualified path to the existing file. 
        /// This can be either maxscript files (*.ms), or maxscript zip files (*.mzp), 
        /// or encrypted maxscript files (*.mse).</param>
        [ComVisible(true)]
        public static void EvaluateMaxScript(string path)
        {
            if (_ExternalMxsListenerHwnd == IntPtr.Zero)
            {
                throw new NullReferenceException("Can't send message, External Maxscript Listener is not found." + Environment.NewLine + @"Did you forget to Call: ""FindExternalMaxscriptListener()""?");
            }
            if (!File.Exists(path))
            {
                throw new ArgumentException("path is not a valid path and can therefor not be evaluated by MAXScript");
            }

            SendMaxMessage(path, MessageType.Path);
        }
        
        /// <summary>
        /// Sends a message to the MAXScript Listener log.
        /// </summary>
        public static void Log(string msg)
        {
            if (_ExternalMxsListenerHwnd == IntPtr.Zero)
            {
                throw new NullReferenceException("Can't send message, External Maxscript Listener is not found." + Environment.NewLine + @"Did you forget to Call: ""FindExternalMaxscriptListener()""?");
            }

            if (msg.Length <= 0)
            {
                throw new ArgumentException("Message string has no characters.", nameof(msg));
            }

            SendMaxMessage(msg, MessageType.Log);
        }

        /// <summary>
        /// Sends the msg to External Maxscript Listener
        /// </summary>
        private static void SendMaxMessage(string msg, MessageType messageTypeOfMsg)
        {
            byte[] buff = Encoding.ASCII.GetBytes(msg);

            COPYDATASTRUCT cds;
            cds.dwData = (IntPtr)messageTypeOfMsg;
            cds.lpData = Marshal.AllocHGlobal(buff.Length);
            Marshal.Copy(buff, 0, cds.lpData, buff.Length);
            cds.cbData = buff.Length;
            
            IntPtr stringResult = SendMessage(_ExternalMxsListenerHwnd, WindowMessages.WM_COPYDATA, 0, ref cds);
            Marshal.FreeHGlobal(cds.lpData);

            if (stringResult == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }
}
