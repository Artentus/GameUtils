using System.Windows.Forms;

namespace GameUtils.Input
{
    public sealed class Mouse : InputSource<MouseState>
    {
        readonly MouseHook hook;
        readonly MouseState state;

        public override MouseState CurrentState
        {
            get { return state; }
        }

        public Mouse()
        {
            hook = new MouseHook();
            hook.MouseDown += MouseDownHandler;
            hook.MouseUp += MouseUpHandler;

            state = new MouseState(hook);
        }

        void MouseDownHandler(object sender, MouseEventArgs e)
        {

        }

        void MouseUpHandler(object sender, MouseEventArgs e)
        {

        }

        public override void ResetHistory()
        {
            
        }

        protected override bool IsCompatibleTo(IEngineComponent component)
        {
            return !(component is Mouse);
        }

        protected override void Dispose(bool disposing)
        {
            hook.Dispose();
        }
    }
}
