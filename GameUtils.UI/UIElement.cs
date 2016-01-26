using System;
using GameUtils.Graphics;
using GameUtils.Math;

namespace GameUtils.UI
{
    /// <summary>
    /// The base class for all controls.
    /// </summary>
    public abstract class UIElement : RegistrationContext<UIState>, IDisposable
    {
        ContainerElement parent;
        Vector2 location;
        Vector2 size;
        bool focused;
        bool visible;

        public event EventHandler LocationChanged;
        public event EventHandler Resized;
        public event EventHandler AbsoluteBoundsChanged;
        public event EventHandler<KeyboardEventArgs> KeyDown;
        public event EventHandler<KeyboardEventArgs> KeyUp;
        public event EventHandler<MouseEventArgs> MouseDown;
        public event EventHandler<MouseEventArgs> MouseUp;
        public event EventHandler<MouseEventArgs> MouseMove;
        public event EventHandler<MouseEventArgs> MouseWheel;
        public event EventHandler MouseEnter;
        public event EventHandler MouseLeave;
        public event EventHandler Click;
        public event EventHandler<PaintEventArgs> Paint;
        public event EventHandler GotFocus;
        public event EventHandler LostFocus;
        public event EventHandler VisibleChanged;
        public event EventHandler ParentChanged;

        /// <summary>
        /// The parent element of this element.
        /// </summary>
        public ContainerElement Parent
        {
            get { return parent; }
            set
            {
                if (parent == value)
                    return;

                if (parent != null && parent.Children.Contains(this))
                    parent.Children.Remove(this);
                else
                    parent = null;

                if (value != null)
                {
                    if (value.Children.Contains(this))
                        parent = value;
                    else
                        value.Children.Add(this);
                }

                this.OnParentChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// The root of this element.
        /// </summary>
        public UIRoot Root
        {
            get
            {
                UIElement element = this;
                while (!(element is UIRoot))
                {
                    if (element.Parent == null)
                        return null;

                    element = element.Parent;
                }
                return (UIRoot)element;
            }
        }

        internal void SetFocusInner(bool value)
        {
            focused = value;
            if (value)
                OnGotFocus(EventArgs.Empty);
            else
                OnLostFocus(EventArgs.Empty);
        }

        /// <summary>
        /// Indicates whether this element holds currently the keyboard focus.
        /// </summary>
        public bool Focused
        {
            get { return focused; }
            set
            {
                this.SetFocusedElement(value ? this : null);
            }
        }

        internal virtual UIElement GetFocusedElement()
        {
            if (parent == null) return null;
            return parent.GetFocusedElement();
        }

        internal virtual void SetFocusedElement(UIElement element)
        {
            if (parent != null && element.CanGetFocus) parent.SetFocusedElement(element);
        }

        /// <summary>
        /// Indicates whether this element is able to get the keyboard focus.
        /// </summary>
        public bool CanGetFocus { get; set; }

        /// <summary>
        /// The position of this element inside his parent.
        /// </summary>
        public Vector2 Location
        {
            get { return this.GetLocationInner(); }
            set { this.SetLocationInner(value); }
        }

        internal virtual Vector2 GetLocationInner()
        {
            return location;
        }

        internal virtual void SetLocationInner(Vector2 value)
        {
            location = value;
            OnLocationChanged(EventArgs.Empty);
        }

        /// <summary>
        /// The absolute location of this element.
        /// </summary>
        public Vector2 AbsoluteLocation
        {
            get { return this.GetAbsoluteLocationInner(); }
        }

        internal virtual Vector2 GetAbsoluteLocationInner()
        {
            if (parent == null)
                return new Vector2(0, 0);

            Vector2 parentSize = Parent.GetAbsoluteSizeInner();
            return new Vector2(parentSize.X * Location.X, parentSize.Y * Location.Y);
        }

        /// <summary>
        /// The size of this element.
        /// </summary>
        public Vector2 Size
        {
            get { return this.GetSizeInner(); }
            set { this.SetSizeInner(value); }
        }

        internal virtual Vector2 GetSizeInner()
        {
            return size;
        }

        internal virtual void SetSizeInner(Vector2 value)
        {
            size = value;
            OnResized(EventArgs.Empty);
        }

        /// <summary>
        /// The absolute size of this element.
        /// </summary>
        public Vector2 AbsoluteSize
        {
            get { return this.GetAbsoluteSizeInner(); }
        }

        internal virtual Vector2 GetAbsoluteSizeInner()
        {
            if (parent == null)
                return new Vector2(0, 0);

            Vector2 parentSize = Parent.GetAbsoluteSizeInner();
            return new Vector2(parentSize.X * Size.X, parentSize.Y * Size.Y);
        }

        /// <summary>
        /// Absolute location and size of this element inside its parent.
        /// </summary>
        public Rectangle AbsoluteBounds
        {
            get
            {
                if (parent == null)
                    return new Rectangle(0, 0, 0, 0);

                Vector2 parentSize = Parent.GetAbsoluteSizeInner();
                return new Rectangle(parentSize.X * Location.X, parentSize.Y * Location.Y, parentSize.X * Size.X, parentSize.Y * Size.Y);
            }
        }

        /// <summary>
        /// Absolute location and size of this element inside the drawing surface.
        /// </summary>
        public Rectangle SurfaceBounds
        {
            get
            {
                Vector2 location = this.GetSurfaceLocationInner();
                Vector2 size = this.GetAbsoluteSizeInner();
                return new Rectangle(location, size);
            }
        }

        internal virtual Vector2 GetSurfaceLocationInner()
        {
            if (parent == null)
                return new Vector2(0, 0);

            Vector2 location = this.GetAbsoluteLocationInner();
            Vector2 parentLocation = parent.GetSurfaceLocationInner();
            return parentLocation + location;
        }

        /// <summary>
        /// Indicates whether this element ist visible.
        /// </summary>
        public bool Visible
        {
            get { return visible; }
            set
            {
                visible = value;
                this.OnVisibleChanged(EventArgs.Empty);
            }
        }

        protected override UIState CreateBuffer()
        {
            return new UIState();
        }

        protected override IStateRenderer<UIState> Renderer
        {
            get { return null; }
        }

        protected UIElement()
        {
            visible = true;
        }

        /// <summary>
        /// Returns the child element at the specfied location or itself if there's no child found.
        /// </summary>
        /// <param name="point">The point at which the element should check for a child relative to the element.</param>
        public UIElement GetChildAtPoint(Vector2 point)
        {
            Vector2 childPoint;
            return this.GetChildAtPointInner(point, out childPoint);
        }

        /// <summary>
        /// Returns the child element at the specfied location or itself if there's no child found.
        /// </summary>
        /// <param name="point">The point at which the element should check for a child relative to the element.</param>
        /// <param name="childPoint">The corresponding point relative to the child.</param>
        public UIElement GetChildAtPoint(Vector2 point, out Vector2 childPoint)
        {
            return GetChildAtPointInner(point, out childPoint);
        }

        internal virtual UIElement GetChildAtPointInner(Vector2 point, out Vector2 childPoint)
        {
            childPoint = point;
            return this;
        }

        /// <summary>
        /// Sets the keyboard focus to this element if possible.
        /// </summary>
        public void Focus()
        {
            Focused = true;
        }

        /// <summary>
        /// Determines if the specified point collides with this element.
        /// </summary>
        /// <param name="point">The point to check. It is to be seen relative to the element.</param>
        protected internal virtual bool HitTest(Vector2 point)
        {
            return new Rectangle(location, size).Contains(point);
        }

        internal virtual void OnApplyChanges(EventArgs e)
        { }

        protected virtual void OnLocationChanged(EventArgs e)
        {
            if (LocationChanged != null) LocationChanged(this, e);

            this.OnAbsoluteBoundsChanged(EventArgs.Empty);
        }

        protected virtual void OnResized(EventArgs e)
        {
            if (Resized != null) Resized(this, e);

            this.OnAbsoluteBoundsChanged(EventArgs.Empty);
        }

        protected internal virtual void OnAbsoluteBoundsChanged(EventArgs e)
        {
            if (AbsoluteBoundsChanged != null) AbsoluteBoundsChanged(this, e);
        }

        protected internal virtual void OnKeyDown(KeyboardEventArgs e)
        {
            if (KeyDown != null) KeyDown(this, e);
        }

        protected internal virtual void OnKeyUp(KeyboardEventArgs e)
        {
            if (KeyUp != null) KeyUp(this, e);
        }

        protected internal virtual void OnMouseDown(MouseEventArgs e)
        {
            if (MouseDown != null) MouseDown(this, e);
        }

        protected internal virtual void OnMouseUp(MouseEventArgs e)
        {
            if (MouseUp != null) MouseUp(this, e);
        }

        protected internal virtual void OnMouseMove(MouseEventArgs e)
        {
            if (MouseMove != null) MouseMove(this, e);
        }

        protected internal virtual void OnMouseEnter(EventArgs e)
        {
            if (MouseEnter != null) MouseEnter(this, e);
        }

        protected internal virtual void OnMouseLeave(EventArgs e)
        {
            if (MouseLeave != null) MouseLeave(this, e);
        }

        protected internal virtual void OnMouseWheel(MouseEventArgs e)
        {
            if (MouseWheel != null) MouseWheel(this, e);
        }

        protected internal virtual void OnClick(EventArgs e)
        {
            if (Click != null) Click(this, e);
        }

        protected internal virtual void OnPaint(PaintEventArgs e)
        {
            if (Paint != null) Paint(this, e);
        }

        protected virtual void OnGotFocus(EventArgs e)
        {
            if (GotFocus != null) GotFocus(this, e);
        }

        protected virtual void OnLostFocus(EventArgs e)
        {
            if (LostFocus != null) LostFocus(this, e);
        }

        protected virtual void OnVisibleChanged(EventArgs e)
        {
            if (VisibleChanged != null) VisibleChanged(this, e);
        }

        protected virtual void OnParentChanged(EventArgs e)
        {
            if (ParentChanged != null) ParentChanged(this, e);
            this.OnAbsoluteBoundsChanged(EventArgs.Empty);
        }

        private bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);

                disposed = true;
            }
        }

        protected virtual void Dispose(bool disposing) { }

        ~UIElement()
        {
            Dispose(false);
        }
    }
}
