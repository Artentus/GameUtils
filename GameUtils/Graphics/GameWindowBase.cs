using System;
using System.Drawing;
using System.Windows.Forms;

namespace GameUtils.Graphics
{
    /// <summary>
    /// The game window provides basic fuctionality for a proper renderer surface.
    /// </summary>
    public abstract class GameWindowBase : Form, ISurface
    {
        readonly bool disableCloseButton;
        bool fullscreen;
        FormBorderStyle oldBorderStyle;
        FormWindowState oldWindowState;
        Rectangle oldBounds;

        /// <summary>
        /// Is risen when the fullscreen mode has changed.
        /// </summary>
        public event EventHandler FullscreenChanged;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams param = base.CreateParams;
                if (disableCloseButton) param.ClassStyle = param.ClassStyle | 0x200;
                return param;
            }
        }

        /// <summary>
        /// Indicates whether the window is in fullscreen mode.
        /// </summary>
        public bool Fullscreen
        {
            get { return fullscreen; }
            set
            {
                if (value != fullscreen)
                {
                    var action = new Action(() =>
                    {
                        fullscreen = value;
                        OnFullscreenChanged(EventArgs.Empty);
                    });

                    Invoke(action);
                }
            }
        }

        bool IEngineComponent.IsCompatibleTo(IEngineComponent component)
        {
            return true;
        }

        /// <summary>
        /// Retrieves the Bounds of this window.
        /// </summary>
        public new Math.Rectangle Bounds
        {
            get
            {
                Func<Rectangle> getClientSize = () => this.ClientRectangle;
                var clientRect = (Rectangle)this.Invoke(getClientSize);
                return new Math.Rectangle(clientRect.X, clientRect.Y, clientRect.Width, clientRect.Height);
            }
        }

        object ISurface.OutputTarget
        {
            get { return this; }
        }

        protected GameWindowBase(bool fullscreen = false, bool disableCloseButton = true)
        {
            SetStyle(ControlStyles.UserPaint, true);
            UpdateStyles();

            this.fullscreen = fullscreen;
            this.disableCloseButton = disableCloseButton;
            if (fullscreen)
            {
                oldBorderStyle = FormBorderStyle;
                oldWindowState = WindowState;
                oldBounds = DesktopBounds;

                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
            }
        }

        protected virtual void OnFullscreenChanged(EventArgs e)
        {
            if (fullscreen)
            {
                oldBorderStyle = FormBorderStyle;
                oldWindowState = WindowState;
                oldBounds = DesktopBounds;

                WindowState = FormWindowState.Normal;
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                FormBorderStyle = oldBorderStyle;
                WindowState = oldWindowState;
                DesktopBounds = oldBounds;
            }

            if (FullscreenChanged != null)
                FullscreenChanged(this, e);
        }

        protected override void OnPaint(PaintEventArgs e)
        { }

        protected override void OnPaintBackground(PaintEventArgs e)
        { }
    }
}
