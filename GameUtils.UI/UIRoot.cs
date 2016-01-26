using System;
using System.Collections.Generic;
using System.Linq;
using GameUtils.Graphics;
using GameUtils.Input;
using GameUtils.Input.DefaultDevices;
using GameUtils.Math;

namespace GameUtils.UI
{
    /// <summary>
    /// The topmost container element.
    /// </summary>
    public sealed class UIRoot : ContainerElement, IEngineComponent, IInputListener<KeyboardState>, IInputListener<MouseState>
    {
        readonly System.Windows.Forms.Control surface;
        readonly ListenerHandle<KeyboardState> keyboardHandle;
        readonly ListenerHandle<MouseState> mouseHandle;
        readonly UIRenderer renderer;
        Key[] pressedKeys;
        MouseState oldMouseState;
        UIElement mouseElement;
        Vector2 mousePos;
        UIElement clickedElement;
        UIElement focusedElement;

        bool IEngineComponent.IsCompatibleTo(IEngineComponent component)
        {
            return true;
        }

        public object Tag { get; set; }

        protected override IStateRenderer<UIState> Renderer
        {
            get { return renderer; }
        }

        public float DepthPosition
        {
            get { return renderer.DepthPosition; }
            set { renderer.DepthPosition = value; }
        }

        /// <summary>
        /// Indicates the element currently owning the keyboard focus.
        /// </summary>
        public UIElement FocusedElement
        {
            get { return focusedElement; }
            set { if (value.CanGetFocus) this.SetFocusedElement(value); }
        }

        internal override UIElement GetFocusedElement()
        {
            return focusedElement;
        }

        internal override void SetFocusedElement(UIElement element)
        {
            if (focusedElement != null)
                focusedElement.SetFocusInner(false);

            focusedElement = element;
            if (element != null)
                element.SetFocusInner(true);
        }

        /// <summary>
        /// The aspect ratio of this root an and all its children or 0 if the ratio shouldn't be kept.
        /// </summary>
        public float AspectRatio { get; set; }

        /// <summary>
        /// Creates a ne root.
        /// </summary>
        public UIRoot(System.Windows.Forms.Control surface, object keyboardTag = null, object mouseTag = null)
        {
            this.surface = surface;
            surface.Resize += (sender, e) => this.OnAbsoluteBoundsChanged(EventArgs.Empty);

            keyboardHandle = GameEngine.QueryComponent<IInputProvider<KeyboardState>>(keyboardTag).RegisterListener(this);
            mouseHandle = GameEngine.QueryComponent<IInputProvider<MouseState>>(mouseTag).RegisterListener(this);

            renderer = new UIRenderer();
            renderer.Render += (sender, e) => this.Render(e.Renderer);

            pressedKeys = new Key[0];
            oldMouseState = new MouseState(MouseButtons.None, 0, 0, 0, 0, 0);
            mouseElement = this;
            mousePos = new Vector2(0, 0);
            clickedElement = null;
        }

        internal override Vector2 GetLocationInner()
        {
            Vector2 location = this.GetAbsoluteLocationInner();
            return new Vector2(location.X / surface.ClientSize.Width, location.Y / surface.ClientSize.Height);
        }

        internal override void SetLocationInner(Vector2 value)
        { }

        internal override Vector2 GetSizeInner()
        {
            Vector2 size = this.GetAbsoluteSizeInner();
            return new Vector2(size.X / surface.ClientSize.Width, size.Y / surface.ClientSize.Height);
        }

        internal override void SetSizeInner(Vector2 value)
        { }

        internal override Vector2 GetAbsoluteLocationInner()
        {
            Vector2 size = this.GetAbsoluteSizeInner();
            return new Vector2((surface.ClientSize.Width - size.X) / 2.0f, (surface.ClientSize.Height - size.Y) / 2.0f);
        }

        internal override Vector2 GetAbsoluteSizeInner()
        {
            if (AspectRatio == 0)
                return new Vector2(surface.ClientSize.Width, surface.ClientSize.Height);
            else if ((float)surface.ClientSize.Width / (float)surface.ClientSize.Height < AspectRatio)
                return new Vector2(surface.ClientSize.Width, surface.ClientSize.Width / AspectRatio);
            else
                return new Vector2(surface.ClientSize.Height * AspectRatio, surface.ClientSize.Height);
        }

        internal override Vector2 GetSurfaceLocationInner()
        {
            return this.GetAbsoluteLocationInner();
        }

        private void Render(Renderer renderer)
        {
            this.OnApplyChanges(EventArgs.Empty);

            Vector2 size = this.GetAbsoluteSizeInner();
            Vector2 location = new Vector2((surface.ClientSize.Width - size.X) / 2.0f, (surface.ClientSize.Height - size.Y) / 2.0f);
            OnPaint(new PaintEventArgs(renderer, new Rectangle(location, size)));
        }

        void IInputListener<KeyboardState>.OnInputReceived(KeyboardState state)
        {
            if (FocusedElement != null)
            {
                bool controlKeyDown = state.IsPressed(Key.LeftControl) || state.IsPressed(Key.RightControl);
                bool altKeyDown = state.IsPressed(Key.LeftAlt) || state.IsPressed(Key.RightAlt);
                bool shiftKeyDown = state.IsPressed(Key.LeftShift) || state.IsPressed(Key.RightShift);
                bool winKeyDown = state.IsPressed(Key.LeftWindowsKey) || state.IsPressed(Key.RightWindowsKey);
                if (state.PressedKeys.Length > pressedKeys.Length)
                {
                    Key key = state.PressedKeys.Except(pressedKeys).First();
                    KeyboardEventArgs args = new KeyboardEventArgs(key, controlKeyDown, altKeyDown, shiftKeyDown, winKeyDown);
                    FocusedElement.OnKeyDown(args);
                }
                else if (state.PressedKeys.Length < pressedKeys.Length)
                {
                    Key key = pressedKeys.Except(state.PressedKeys).First();
                    KeyboardEventArgs args = new KeyboardEventArgs(key, controlKeyDown, altKeyDown, shiftKeyDown, winKeyDown);
                    FocusedElement.OnKeyUp(args);
                }
                pressedKeys = state.PressedKeys;
            }
        }

        private Vector2 GetChildPoint(UIElement child, float x, float y)
        {
            var parentTree = new Stack<UIElement>();
            parentTree.Push(child);
            while (parentTree.Peek() != this)
                parentTree.Push(parentTree.Peek().Parent);

            while (parentTree.Count > 0)
            {
                UIElement element = parentTree.Pop();
                x -= element.Location.X;
                y -= element.Location.Y;
                x /= element.Size.X;
                y /= element.Size.Y;
            }

            return new Vector2(x, y);
        }

        void IInputListener<MouseState>.OnInputReceived(MouseState state)
        {
            Vector2 absoluteLocation = this.GetAbsoluteLocationInner();
            Vector2 absoluteSize = this.GetAbsoluteSizeInner();
            float x = (state.AbsoluteX - absoluteLocation.X) / absoluteSize.X, y = (state.AbsoluteY - absoluteLocation.Y) / absoluteSize.Y;

            //mouse was moved
            if (state.RelativeX != 0 || state.RelativeY != 0)
            {
                if (clickedElement != null)
                {
                    mousePos = this.GetChildPoint(clickedElement, x, y);
                    clickedElement.OnMouseMove(new MouseEventArgs(state.PressedButtons, mousePos, 0));
                }
                else
                {
                    UIElement newMouseElement = GetChildAtPoint(new Vector2(x, y), out mousePos);
                    if (newMouseElement != mouseElement)
                    {
                        mouseElement.OnMouseLeave(EventArgs.Empty);
                        newMouseElement.OnMouseEnter(EventArgs.Empty);
                        mouseElement = newMouseElement;
                    }
                    else
                        mouseElement.OnMouseMove(new MouseEventArgs(state.PressedButtons, mousePos, 0));
                }
            }
            
            //scrolled
            if (state.Delta != 0)
            {
                if (clickedElement != null)
                    clickedElement.OnMouseWheel(new MouseEventArgs(state.PressedButtons, mousePos, state.Delta));
                else
                    mouseElement.OnMouseWheel(new MouseEventArgs(state.PressedButtons, mousePos, state.Delta));
            }

            MouseButtons pressedButton = state.PressedButtons & ~oldMouseState.PressedButtons;
            MouseButtons releasedButton = oldMouseState.PressedButtons & ~state.PressedButtons;

            //mouse button pressed
            if (pressedButton != MouseButtons.None)
            {
                if (clickedElement != null)
                    clickedElement.OnMouseDown(new MouseEventArgs(pressedButton, mousePos, 0));
                else
                {
                    clickedElement = mouseElement;
                    mouseElement.OnMouseDown(new MouseEventArgs(pressedButton, mousePos, 0));
                    mouseElement.Focus();
                }
            }

            //mouse button released
            if (releasedButton != MouseButtons.None && clickedElement != null)
            {
                clickedElement.OnMouseUp(new MouseEventArgs(releasedButton, mousePos, 0));
                if (releasedButton == MouseButtons.Left) clickedElement.OnClick(EventArgs.Empty);

                if (state.PressedButtons == MouseButtons.None)
                {
                    UIElement newMouseElement = GetChildAtPoint(new Vector2(x, y), out mousePos);
                    if (newMouseElement != clickedElement)
                    {
                        clickedElement.OnMouseLeave(EventArgs.Empty);
                        newMouseElement.OnMouseEnter(EventArgs.Empty);
                        mouseElement = newMouseElement;
                    }

                    clickedElement = null;
                }
            }

            oldMouseState = state;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                pressedKeys = null;
                oldMouseState = null;
            }

            keyboardHandle.Dispose();
            mouseHandle.Dispose();
        }

        internal class UIRenderer : IStateRenderer<UIState>
        {
            float depthPosition;

            public event EventHandler DepthPositionChanged;
            internal event EventHandler<RenderEventArgs> Render;

            public float DepthPosition
            {
                get { return depthPosition; }
                set
                {
                    if (value != depthPosition)
                    {
                        depthPosition = value;
                        if (DepthPositionChanged != null)
                            DepthPositionChanged(this, EventArgs.Empty);
                    }
                }
            }

            void IStateRenderer<UIState>.Render(UIState state, Renderer renderer)
            {
                if (Render != null)
                    Render(this, new RenderEventArgs(renderer));
            }
        }

        internal class RenderEventArgs : EventArgs
        {
            public Renderer Renderer { get; private set; }

            public RenderEventArgs(Renderer renderer)
            {
                Renderer = renderer;
            }
        }
    }
}
