using System.Windows.Forms;

namespace GameUtils.Input
{
    public sealed class Keyboard : RecordableInputSource<KeyboardState, KeyboardRecorder>
    {
        readonly KeyboardHook hook;
        readonly KeyboardState state;

        public override KeyboardState CurrentState
        {
            get { return state; }
        }

        public Keyboard()
        {
            hook = new KeyboardHook();
            hook.KeyDown += KeyDownHandler;
            hook.KeyUp += KeyUpHandler;

            state = new KeyboardState(hook);
        }

        void KeyDownHandler(object sender, KeyEventArgs e)
        {
            
        }

        void KeyUpHandler(object sender, KeyEventArgs e)
        {

        }

        protected override KeyboardRecorder CreateRecorderInner()
        {
            return new KeyboardRecorder();
        }

        protected override bool IsCompatibleTo(IEngineComponent component)
        {
            return !(component is Keyboard);
        }

        protected override void Dispose(bool disposing)
        {
            hook.Dispose();
        }
    }
}
