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
        volatile float dpi;
        FormBorderStyle oldBorderStyle;
        FormWindowState oldWindowState;
        Rectangle oldBounds;

        /// <summary>
        /// Is risen when the fullscreen mode has changed.
        /// </summary>
        public event EventHandler FullscreenChanged;

        /// <summary>
        /// Is risen when the DPI have changed.
        /// </summary>
        public event EventHandler DpiChanged;

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

        /// <summary>
        /// The DPI of the window.
        /// The default value is 96.
        /// </summary>
        public float Dpi
        {
            get { return dpi; }
            set
            {
                if (value != dpi)
                {
                    dpi = value;
                    OnDpiChanged(EventArgs.Empty);
                }
            }
        }

        bool IEngineComponent.IsCompatibleTo(IEngineComponent component)
        {
            return true;
        }

        Math.Rectangle ISurface.Bounds
        {
            get
            {
                Func<Rectangle> getClientSize = () => this.ClientRectangle;
                var clientRect = (Rectangle)this.Invoke(getClientSize);
                return new Math.Rectangle(clientRect.X, clientRect.Y, clientRect.Width, clientRect.Height);
            }
        }

        object ISurface.OutputTarget => this;

        protected GameWindowBase(bool fullscreen = false, bool disableCloseButton = true)
        {
            SetStyle(ControlStyles.UserPaint, true);
            UpdateStyles();

            this.fullscreen = fullscreen;
            this.dpi = 96;
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

            FullscreenChanged?.Invoke(this, e);
        }

        protected virtual void OnDpiChanged(EventArgs e)
        {
            DpiChanged?.Invoke(this, e);
        }

        protected override void OnPaint(PaintEventArgs e)
        { }

        protected override void OnPaintBackground(PaintEventArgs e)
        { }
    }
}
