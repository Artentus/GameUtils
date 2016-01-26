using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GameUtils.Input
{
    public sealed class MouseState : IInputState
    {
        readonly object locker;
        readonly Dictionary<MouseButton, bool> states;

        internal MouseState(MouseHook hook)
        {
            locker = new object();

            states = new Dictionary<MouseButton, bool>();
            foreach (MouseButton button in Enum.GetValues(typeof(MouseButton)))
                states.Add(button, false);

            hook.MouseDown += MouseDownHandler;
            hook.MouseUp += MouseUpHandler;
        }

        void MouseDownHandler(object sender, MouseEventArgs e)
        {
            lock (locker)
            {
                states[(MouseButton)e.Button] = true;
            }
        }
        
        void MouseUpHandler(object sender, MouseEventArgs e)
        {
            lock (locker)
            {
                states[(MouseButton)e.Button] = false;
            }
        }

        public bool IsPressed(MouseButton button)
        {
            lock (locker)
            {
                return states[button];
            }
        }

        IEnumerable IInputState.EnumerateElements()
        {
            return Enum.GetValues(typeof(MouseButton));
        }

        ElementType IInputState.GetElementType(object element)
        {
            if (!(element is MouseButton))
                throw new ArgumentException("Argument must be of type 'GameUtils.Input.MouseButton'.", "element");

            return ElementType.Digital;
        }

        float IInputState.GetElementState(object element)
        {
            if (!(element is MouseButton))
                throw new ArgumentException("Argument must be of type 'GameUtils.Input.MouseButton'.", "element");

            lock (locker)
            {
                return states[(MouseButton)element] ? 1f : 0f;
            }
        }
    }
}
