using System.Diagnostics;

namespace GameUtils.Logging
{
    public sealed class VisualStudioTrace : Logger
    {
        protected override bool IsCompatibleTo(IEngineComponent component)
        {
            return !(component is VisualStudioTrace);
        }

        public override void SendToOutput(string message)
        {
            Trace.WriteLine(message);
        }
    }
}
