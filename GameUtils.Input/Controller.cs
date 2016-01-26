using System.Timers;
using SharpDX.XInput;

namespace GameUtils.Input
{
    public sealed class Controller : InputSource<ControllerState>
    {
        const int ControllerCount = 4;
        static Controller[] controllers;

        public static Controller[] GetControllers()
        {
            if (controllers == null)
            {
                controllers = new Controller[ControllerCount];
                for (int i = 0; i < ControllerCount; i++)
                    controllers[i] = new Controller(i);
            }
            return controllers;
        }

        readonly SharpDX.XInput.Controller xboxController;
        readonly Timer timer;

        public override ControllerState CurrentState
        {
            get
            {
                if (IsConnected)
                {
                    State state;
                    if (xboxController.GetState(out state))
                        return new ControllerState(state.Gamepad);
                    else
                        IsConnected = false;
                }
                return new ControllerState(default(Gamepad));
            }
        }

        public bool IsConnected { get; private set; }

        public ControllerType Type { get; private set; }

        private Controller(int index)
        {
            xboxController = new SharpDX.XInput.Controller((UserIndex)index);
            CheckConnection();

            timer = new Timer(5);
            timer.Elapsed += (sender, e) => CheckConnection();
            timer.Start();
        }

        private void CheckConnection()
        {
            if (IsConnected) return;

            Capabilities caps;
            if (xboxController.GetCapabilities(DeviceQueryType.Any, out caps))
            {
                IsConnected = true;
                Type = (ControllerType)caps.SubType;
            }
        }

        protected override bool IsCompatibleTo(IEngineComponent component)
        {
            return true;
        }
    }
}
