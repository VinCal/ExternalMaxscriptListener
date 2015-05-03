using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace SendMaxscript
{
    class Program
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", SetLastError = true, EntryPoint = "SendMessage")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);

        //Used to define private messages for use by private window classes, usually of the form WM_USER+x, where x is an integer value.
        private const int WM_USER = 0x0400;

        //Message numbers in the third range (0x8000 through 0xBFFF) are available for applications to use as private messages. 
        //Messages in this range do not conflict with system messages.
        private const int WM_USER_INITIALIZE = WM_USER + 0x8000;

        //Message used with COPYDATASTRUCT
        private const int WM_COPYDATA = 0x4A;

        //Used for WM_COPYDATA for string messages
        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        private static IntPtr _ExternalMxsListenerHwnd;

        static void Main(string[] args)
        {
            bool result = FindExternalMaxscriptListener();

            if (result)
            {
                Console.WriteLine(@"Type: ""exit"" to exit this application.");
                Console.WriteLine("Enter Maxscript commands:");

                while (true)
                {
                    string line = Console.ReadLine(); // Get string from user
                    if (line == "exit") // Check string
                        break;

                    //Send Maxscript command or ms file path to the External Maxscript Listener plugin
                    SendMaxscript(line);
                }
            }
            else
            {
                Console.WriteLine("Unable to find External Maxscript Listener window");
            }

            Console.ReadLine();
        }

        private static bool FindExternalMaxscriptListener()
        {
            _ExternalMxsListenerHwnd = FindWindowByCaption(IntPtr.Zero, "External Maxscript Listener");
            if (_ExternalMxsListenerHwnd != IntPtr.Zero)
            {
                //Send initialize message to check if it's our window and not a random one with the exact same name...(unlikely :D)
                IntPtr resultPtr = SendMessage(_ExternalMxsListenerHwnd, (uint) WM_USER_INITIALIZE, (IntPtr) 1,IntPtr.Zero);
                int initResult = resultPtr.ToInt32();

                if (initResult == 1) return true;
            }

            return false;
        }

        private static void SendMaxscript(string msg)
        {
            byte[] buff = Encoding.ASCII.GetBytes(msg);

            COPYDATASTRUCT cds;
            cds.dwData = (IntPtr)100;
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
