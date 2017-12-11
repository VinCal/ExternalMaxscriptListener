using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Autodesk.Max;
using MAXScriptParserTypes;

namespace ExternalMaxscript.Listener
{
    /// <inheritdoc />
    /// <summary>
    /// ExternalMaxscriptListener is only used to send messages to from any other process and it will then Evaluate the file or Execute the maxscript command
    /// </summary>
    public sealed class ExternalMaxscriptListener : Form
    {
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
                case WindowMessages.WM_USER_INITIALIZE:
                    m.Result = (IntPtr)1;
                    return;

                case WindowMessages.WM_COPYDATA:
                    var cds = (COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(COPYDATASTRUCT)); 
                    var buff = new byte[cds.cbData];
                    Marshal.Copy(cds.lpData, buff, 0, cds.cbData);
                    string msg = Encoding.ASCII.GetString(buff, 0, cds.cbData);
                    IntPtr type = cds.dwData;

                    // I think we should move this somewhere else 
                    switch ((MessageType)type)
                    {
                        case MessageType.Command:
                            GlobalInterface.Instance.ExecuteMAXScriptScript(msg, false, null);
                            break;
                        case MessageType.Path:
                            string errorLog = string.Empty;
                            bool result = GlobalInterface.Instance.FileinScriptEx(msg, errorLog);
                            if (!result)
                            {
                                Log(errorLog, true);
                            }
                            break;
                        case MessageType.Log:
                            Log(msg, true);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    m.Result = (IntPtr)1;
                    return;
            }

            base.WndProc(ref m); 
        }

        /// <summary>
        /// Logs the message to the MAXScript Listener.
        /// </summary>
        public static void Log(string message, bool newLine)
        {
            GlobalInterface.Instance.TheListener.EditStream.Printf(message + (newLine ? Environment.NewLine : string.Empty), null);
        }
    }
}
