using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Autodesk.Max;

namespace ExternalMaxscript
{
    /// <summary>
    /// Autodesk.Max Plugin class, gets loaded when Max loads. Hide the form and disable ShowInTaskbar.
    /// </summary>
    public class ExternalMaxscriptListner : Autodesk.Max.IPlugin
    {
        private ExternalMaxscriptListener _Dialog;

        public void Cleanup()
        {
            _Dialog.Close();
            _Dialog.Dispose();
        }

        public void Initialize(IGlobal global, ISynchronizeInvoke sync)
        {
            _Dialog = new ExternalMaxscriptListener();
            _Dialog.Show();
            _Dialog.Hide();
            _Dialog.ShowInTaskbar = false;
        }
    }

    /// <summary>
    /// ExternalMaxscriptListener is only used to send messages to from any other process and it will then Evaluate the file or Execute the maxscript command
    /// </summary>
    public class ExternalMaxscriptListener : Form
    {
        private const int WM_USER = 0x0400;
        private const int WM_USER_INITIALIZE = WM_USER + 0x8000;
        private const int WM_COPYDATA = 0x4A;

        //Used for WM_COPYDATA for string messages
        [StructLayout(LayoutKind.Sequential)]
        private struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        private const string Command = "_COMM_";
        private const string Path = "_PATH_";
        private const int TypeLength = 6;

        public ExternalMaxscriptListener()
        {
            ClientSize = new System.Drawing.Size(1, 1);
            Name = "MxsListener";
            Text = "External Maxscript Listener";
        }
        
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_USER_INITIALIZE:
                    m.Result = (IntPtr)1;
                    return;

                case WM_COPYDATA:
                    var cds = (COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(COPYDATASTRUCT)); 
                    var buff = new byte[cds.cbData];
                    Marshal.Copy(cds.lpData, buff, 0, cds.cbData);
                    string msg = Encoding.ASCII.GetString(buff, 0, cds.cbData);

                    string type = msg.Substring(0, TypeLength);
                    msg = msg.Substring(TypeLength);

                    if (type == Command)
                    {
                        GlobalInterface.Instance.ExecuteMAXScriptScript(msg, false, null);
                    }
                    else if (type == Path)
                    {
                        string errorLog = String.Empty;
                        bool result = GlobalInterface.Instance.FileinScriptEx(msg, errorLog);
                        if (!result)
                        {
                            Log(errorLog, true);
                        }
                    }

                    m.Result = (IntPtr)1;
                    return;
            }

            base.WndProc(ref m); 
        }

        public static void Log(string message, bool newLine)
        {
            GlobalInterface.Instance.TheListener.EditStream.Printf(message + (newLine ? Environment.NewLine : string.Empty), null);
        }
    }
}
