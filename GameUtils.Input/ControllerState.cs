using System;
using System.Collections;
using GameUtils.Math;
using SharpDX.XInput;

namespace GameUtils.Input
{
    public sealed class ControllerState : IInputState
    {
        readonly Gamepad state;

        internal ControllerState(Gamepad state)
        {
            this.state = state;
        }

        public bool IsPressed(ControllerButton button)
        {
            switch (button)
            {
                case ControllerButton.LeftTrigger:
                    return state.LeftTrigger > Gamepad.TriggerThreshold;
                case ControllerButton.RightTrigger:
                    return state.RightTrigger > Gamepad.TriggerThreshold;
                case ControllerButton.LeftThumbX:
                case ControllerButton.LeftThumbY:
                case ControllerButton.RightThumbX:
                case ControllerButton.RightThumbY:
                    throw new ArgumentException("The thumbsticks are invalid arguments for this function.");
                default:
                    return state.Buttons.HasFlag((GamepadButtonFlags)button);
            }
        }

        float CalculateNormalizedMagnitude(short value, short deadzone)
        {
            if (value < deadzone) return 0f;
            return MathHelper.Clamp((value - deadzone) / (32767f - deadzone), -1f, 1f);
        }

        public float GetAnalogState(ControllerButton button)
        {
            switch (button)
            {
                case ControllerButton.LeftTrigger:
                    return state.LeftTrigger / 255f;
                case ControllerButton.RightTrigger:
                    return state.RightTrigger / 255f;
                case ControllerButton.LeftThumbX:
                    return CalculateNormalizedMagnitude(state.LeftThumbX, Gamepad.LeftThumbDeadZone);
                case ControllerButton.LeftThumbY:
                    return CalculateNormalizedMagnitude(state.LeftThumbY, Gamepad.LeftThumbDeadZone);
                case ControllerButton.RightThumbX:
                    return CalculateNormalizedMagnitude(state.RightThumbX, Gamepad.RightThumbDeadZone);
                case ControllerButton.RightThumbY:
                    return CalculateNormalizedMagnitude(state.RightThumbY, Gamepad.RightThumbDeadZone);
                default:
                    throw new ArgumentException("Only the thumbsticks and triggers are valid arguments for this function.");
            }
        }

        public override IEnumerable EnumerateElements()
        {
            return Enum.GetValues(typeof(ControllerButton));
        }

        public override ElementType GetElementType(object element)
        {
            if (!(element is ControllerButton))
                throw new ArgumentException("Argument must be of type 'GameUtils.Input.ControllerButton'.", "element");

            ControllerButton button = (ControllerButton)element;
            if (button == ControllerButton.LeftTrigger || button == ControllerButton.RightTrigger
                || button == ControllerButton.RightThumbX || button == ControllerButton.RightThumbY
                || button == ControllerButton.LeftThumbX || button == ControllerButton.LeftThumbY)
            {
                return ElementType.Analog;
            }
            else
            {
                return ElementType.Digital;
            }
        }

        public override float GetElementState(object element)
        {
            if (!(element is ControllerButton))
                throw new ArgumentException("Argument must be of type 'GameUtils.Input.ControllerButton'.", "element");

            ControllerButton button = (ControllerButton)element;
            switch (button)
            {
                case ControllerButton.LeftTrigger:
                    return state.LeftTrigger / 255f;
                case ControllerButton.RightTrigger:
                    return state.RightTrigger / 255f;
                case ControllerButton.LeftThumbX:
                    return CalculateNormalizedMagnitude(state.LeftThumbX, Gamepad.LeftThumbDeadZone);
                case ControllerButton.LeftThumbY:
                    return CalculateNormalizedMagnitude(state.LeftThumbY, Gamepad.LeftThumbDeadZone);
                case ControllerButton.RightThumbX:
                    return CalculateNormalizedMagnitude(state.RightThumbX, Gamepad.RightThumbDeadZone);
                case ControllerButton.RightThumbY:
                    return CalculateNormalizedMagnitude(state.RightThumbY, Gamepad.RightThumbDeadZone);
                default:
                    return state.Buttons.HasFlag((GamepadButtonFlags)button) ? 1f : 0f;
            }
        }
    }
}
