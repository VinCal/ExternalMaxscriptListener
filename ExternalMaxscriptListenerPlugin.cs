using System.ComponentModel;
using Autodesk.Max;

namespace ExternalMaxscriptListener
{
    /// <inheritdoc />
    /// <summary>
    /// Autodesk.Max Plugin class, gets loaded when Max loads. Hide the form and disable ShowInTaskbar.
    /// </summary>
    public sealed class ExternalMaxscriptListenerPlugin : IPlugin
    {
        private ExternalMaxscript.Listener.ExternalMaxscriptListener _dialog;

        public void Cleanup()
        {
            _dialog.Close();
            _dialog.Dispose();
        }

        public void Initialize(IGlobal global, ISynchronizeInvoke sync)
        {
            _dialog = new ExternalMaxscript.Listener.ExternalMaxscriptListener();
            _dialog.Show();
            _dialog.Hide();
            _dialog.ShowInTaskbar = false;
        }
    }
}