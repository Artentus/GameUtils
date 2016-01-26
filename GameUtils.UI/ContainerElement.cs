using GameUtils.Math;

namespace GameUtils.UI
{
    /// <summary>
    /// An element serving as container for other elements.
    /// </summary>
    public abstract class ContainerElement : UIElement
    {
        /// <summary>
        /// The children of this container element.
        /// </summary>
        public UIElementCollection Children { get; private set; }

        protected ContainerElement()
        {
            Children = new UIElementCollection(this);
        }

        internal override UIElement GetChildAtPointInner(Vector2 point, out Vector2 childPoint)
        {
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                UIElement child = Children[i];
                if (child.Visible && child.HitTest(point))
                {
                    UIElement result = child.GetChildAtPoint(new Vector2((point.X - child.Location.X) / child.Size.X, (point.Y - child.Location.Y) / child.Size.Y), out childPoint);
                    return result;
                }
            }

            return base.GetChildAtPointInner(point, out childPoint);
        }

        protected internal override void OnAbsoluteBoundsChanged(System.EventArgs e)
        {
            base.OnAbsoluteBoundsChanged(e);

            for (int i = 0; i < Children.Count; i++)
                Children[i].OnAbsoluteBoundsChanged(e);
        }

        protected internal override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            for (int i = 0; i < Children.Count; i++)
            {
                UIElement child = Children[i];
                if (child.Visible)
                {
                    Rectangle bounds = new Rectangle(e.Bounds.X + child.AbsoluteLocation.X, e.Bounds.Y + child.AbsoluteLocation.Y, child.AbsoluteSize.X, child.AbsoluteSize.Y);
                    PaintEventArgs args = new PaintEventArgs(e.Renderer, bounds);
                    child.OnPaint(args);
                }
            }
        }

        internal override void OnApplyChanges(System.EventArgs e)
        {
            Children.ApplyChanges();

            for (int i = 0; i < Children.Count; i++)
                Children[i].OnApplyChanges(e);
        }
    }
}
