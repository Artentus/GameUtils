using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GameUtils.Input
{
    public sealed class KeyboardState : IInputState
    {
        readonly object locker;
        readonly Dictionary<Key, bool> states;

        internal KeyboardState(KeyboardHook hook)
        {
            locker = new object();

            states = new Dictionary<Key, bool>();
            foreach (Key key in Enum.GetValues(typeof(Key)))
                states.Add(key, false);

            hook.KeyDown += KeyDownHandler;
            hook.KeyUp += KeyUpHandler;
        }

        void KeyDownHandler(object sender, KeyEventArgs e)
        {
            lock (locker)
            {
                states[(Key)e.KeyCode] = true;
            }
        }

        void KeyUpHandler(object sender, KeyEventArgs e)
        {
            lock (locker)
            {
                states[(Key)e.KeyCode] = false;
            }
        }

        public bool IsPressed(Key key)
        {
            lock (locker)
            {
                if (key == Key.Shift) return states[Key.LShiftKey] || states[Key.RShiftKey];
                if (key == Key.Control) return states[Key.LControlKey] || states[Key.RControlKey];
                if (key == Key.Alt) return states[Key.LMenu] || states[Key.RMenu];

                return states[key];
            }
        }

        IEnumerable IInputState.EnumerateElements()
        {
            return Enum.GetValues(typeof(Key));
        }

        ElementType IInputState.GetElementType(object element)
        {
            if (!(element is Key))
                throw new ArgumentException("Argument must be of type 'GameUtils.Input.Key'.", "element");

            return ElementType.Digital;
        }

        float IInputState.GetElementState(object element)
        {
            if (!(element is Key))
                throw new ArgumentException("Argument must be of type 'GameUtils.Input.Key'.", "element");

            lock (locker)
            {
                return states[(Key)element] ? 1f : 0f;
            }
        }
    }
}
