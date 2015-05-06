using System;
using System.ComponentModel;
using System.IO;
using System.Net.Configuration;
using System.Runtime.InteropServices;
using System.Text;

namespace ExternalMaxscript
{
    public static class ExternalMaxscriptSender
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        private static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", SetLastError = true, EntryPoint = "SendMessage")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);

        //Used to define private messages for use by private window classes, usually of the form WM_USER+x, where x is an integer value.
        private const int WM_USER = 0x0400;

        //Message numbers in the third range (0x8000 through 0xBFFF) are available for applications to use as private messages. 
        //Messages in this range do not conflict with system messages.
        private const int WM_USER_INITIALIZE = WM_USER + 0x8000;

        //Message used with COPYDATASTRUCT
        private const int WM_COPYDATA = 0x4A;

        //Used for WM_COPYDATA for string messages
        [StructLayout(LayoutKind.Sequential)]
        private struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        private enum Type
        {
            Command,
            Path
        };

        //Data-member to keep track of the Window handle for our External Maxscript Listener window. 
        private static IntPtr _ExternalMxsListenerHwnd;

        /// <summary>
        /// This methods should be called once to connect to the External Maxscript Listener plugin loaded by 3ds Max.
        /// </summary>
        /// <returns>True if connecting and sending the WM_USER_INITIALIZE message was successful, false otherwise</returns>
        public static bool Initialize()
        {
            _ExternalMxsListenerHwnd = FindWindowByCaption(IntPtr.Zero, "External Maxscript Listener");
            if (_ExternalMxsListenerHwnd != IntPtr.Zero)
            {
                //Send initialize message to check if it's our window and not a random one with the exact same name...(unlikely :D)
                IntPtr resultPtr = SendMessage(_ExternalMxsListenerHwnd, (uint)WM_USER_INITIALIZE, (IntPtr)1, IntPtr.Zero);
                int initResult = resultPtr.ToInt32();

                if (initResult == 1) return true;
            }

            return false;
        }

        /// <summary>
        /// Sends msg to External Maxscript Listener, which in turn executes the maxscript code.
        /// </summary>
        /// <param name="msg">Points to a null-terminated string that specifies the MAXScript commands to compile and evaluate. This expects a string containing Maxscript expressions, NOT a file path.</param>
        public static void ExecuteMAXScriptScript(string msg)
        {
            if (_ExternalMxsListenerHwnd == IntPtr.Zero)
            {
                throw new NullReferenceException("Can't send message, External Maxscript Listener is not found." + Environment.NewLine + @"Did you forget to Call: ""FindExternalMaxscriptListener()""?");
            }

            if (msg.Length <= 0)
            {
                throw new ArgumentException("Message string has no characters.", "msg");
            }

            SendMessage(msg, Type.Command);
        }

        /// <summary>
        /// Send path to External Maxscript Listener, which in turn evaluates the script at the given location.
        /// </summary>
        /// <param name="path">The fully qualified path to the existing file. This can be either maxscript files (*.ms), or maxscript zip files (*.mzp), or encrypted maxscript files (*.mse).</param>
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

            SendMessage(path, Type.Path);
        }

        /// <summary>
        /// Sends the msg to External Maxscript Listener
        /// </summary>
        private static void SendMessage(string msg, Type typeOfMsg)
        {
            byte[] buff = Encoding.ASCII.GetBytes(msg);

            COPYDATASTRUCT cds;
            cds.dwData = (IntPtr)typeOfMsg;
            cds.lpData = Marshal.AllocHGlobal(buff.Length);
            Marshal.Copy(buff, 0, cds.lpData, buff.Length);
            cds.cbData = buff.Length;

            IntPtr stringResult = SendMessage(_ExternalMxsListenerHwnd, WM_COPYDATA, 0, ref cds);

            Marshal.FreeHGlobal(cds.lpData);

            if (stringResult == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }
}
