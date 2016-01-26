using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.Direct2D1;
using Cursor = System.Windows.Forms.Cursor;
using Cursors = System.Windows.Forms.Cursors;
using DXGI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;
using D2D = SharpDX.Direct2D1;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace MagicMesh
{
    class MeshEditor : WindowsFormsHost
    {
        public static readonly DependencyProperty ProjectProperty = DependencyProperty.Register("Project",
            typeof(Project),
            typeof(MeshEditor));

        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register("Mode",
            typeof(EditorMode),
            typeof(MeshEditor));

        public static readonly DependencyProperty TranslationProperty = DependencyProperty.Register("Translation",
            typeof(Vector2),
            typeof(MeshEditor),
            new PropertyMetadata(OnTransformChanged));

        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register("Scale",
            typeof(float),
            typeof(MeshEditor),
            new PropertyMetadata(OnTransformChanged));

        static void OnTransformChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((MeshEditor)obj).OnTransformChanged();
        }

        readonly Cursor grabCursor;
        readonly Cursor grabbingCursor;

        D2D.Factory1 d2dFactory;
        D2D.Device d2dDevice;
        D2D.DeviceContext d2dDeviceContext;
        Bitmap1 d2dSurface;
        D3D.Device d3dDevice;
        DXGI.Device1 dxgiDevice;
        SwapChain1 swapChain;

        Brush VertexFillBrush;
        Brush VertexDrawBrush;
        Brush EdgeDrawBrush;
        Brush RasterDrawBrush;

        bool resized;

        MouseButtons pressedMouseButtons;
        bool dragging, wasDragging;
        float oldMouseX, oldMouseY;

        public Project Project
        {
            get { return (Project)base.GetValue(ProjectProperty); }
            set { base.SetValue(ProjectProperty, value); }
        }

        public EditorMode Mode
        {
            get { return (EditorMode)base.GetValue(ModeProperty); }
            set { base.SetValue(ModeProperty, value); }
        }

        public Vector2 Translation
        {
            get { return (Vector2)base.GetValue(TranslationProperty); }
            set { base.SetValue(TranslationProperty, value); }
        }

        public float Scale
        {
            get { return (float)base.GetValue(ScaleProperty); }
            set { base.SetValue(ScaleProperty, value); }
        }

        static Cursor LoadCursor(byte[] resource)
        {
            using (var stream = new MemoryStream(resource))
            {
                return new Cursor(stream);
            }
        }

        public MeshEditor()
        {
            grabCursor = LoadCursor(Properties.Resources.grab);
            grabbingCursor = LoadCursor(Properties.Resources.grabbing);

            Child = new RenderPanel();
            InitializeDirect2D();
            Child.Resize += (sender, e) => resized = true;
            Child.Paint += (sender, e) => Render();

            Child.MouseDown += MouseDownHandler;
            Child.MouseUp += MouseUpHandler;
            Child.MouseMove += MouseMoveHandler;
            Child.MouseWheel += MouseWheelHandler;
        }

        void MouseDownHandler(object sender, MouseEventArgs e)
        {
            pressedMouseButtons |= e.Button;

            if (pressedMouseButtons.HasFlag(MouseButtons.Left | MouseButtons.Right))
            {
                dragging = true;
                wasDragging = true;
                oldMouseX = e.X;
                oldMouseY = e.Y;
                Child.Cursor = grabbingCursor;
            }
        }

        void MouseUpHandler(object sender, MouseEventArgs e)
        {
            pressedMouseButtons &= ~e.Button;

            if (!pressedMouseButtons.HasFlag(MouseButtons.Left | MouseButtons.Right))
            {
                dragging = false;
                Child.Cursor = Cursors.Arrow;
            }

            if (!wasDragging) Mode.MouseClicked(new Vector2(e.X,e.Y));
            if (pressedMouseButtons == MouseButtons.None) wasDragging = false;
        }

        void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Translation += new Vector2(e.X - oldMouseX, e.Y - oldMouseY) / Scale;
                oldMouseX = e.X;
                oldMouseY = e.Y;
            }
        }

        void MouseWheelHandler(object sender, MouseEventArgs e)
        {
            Scale += e.Delta / 120f * 0.05f;
        }

        void OnTransformChanged()
        {
            Render();
        }

        void InitializeDirect2D()
        {
            d3dDevice = new D3D.Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport);
            dxgiDevice = d3dDevice.QueryInterface<DXGI.Device1>();
            var desc = new SwapChainDescription1()
            {
                Width = 0,
                Height = 0,
                Format = Format.B8G8R8A8_UNorm,
                Stereo = false,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.RenderTargetOutput,
                BufferCount = 3,
                Scaling = Scaling.None,
                SwapEffect = SwapEffect.FlipSequential,
                Flags = SwapChainFlags.None
            };
            DXGI.Factory2 dxgiFactory = dxgiDevice.Adapter.GetParent<DXGI.Factory2>();
            swapChain = new SwapChain1(dxgiFactory, d3dDevice, Child.Handle, ref desc);
            swapChain.BackgroundColor = Color4.White;
            dxgiFactory.Dispose();

            d2dFactory = new D2D.Factory1(FactoryType.SingleThreaded);
            d2dDevice = new D2D.Device(d2dFactory, dxgiDevice);
            d2dDeviceContext = new D2D.DeviceContext(d2dDevice, DeviceContextOptions.None);
            d2dDeviceContext.TextAntialiasMode = TextAntialiasMode.Cleartype;
            //d2dDeviceContext.DotsPerInch = new Size2F(96, 96);
            var props = new BitmapProperties1(new PixelFormat(Format.B8G8R8A8_UNorm, D2D.AlphaMode.Ignore),
                d2dDeviceContext.DotsPerInch.Width,
                d2dDeviceContext.DotsPerInch.Height,
                BitmapOptions.Target | BitmapOptions.CannotDraw);
            Surface1 dxgiSurface = swapChain.GetBackBuffer<Surface1>(0);
            d2dSurface = new Bitmap1(d2dDeviceContext, dxgiSurface, props);
            dxgiSurface.Dispose();
            d2dDeviceContext.Target = d2dSurface;

            VertexFillBrush = new SolidColorBrush(d2dDeviceContext, new Color4(1, 0.5f, 0, 1));
            VertexDrawBrush = new SolidColorBrush(d2dDeviceContext, new Color4(0.2f, 0.2f, 0.2f, 1));
            EdgeDrawBrush = new SolidColorBrush(d2dDeviceContext, Color4.Black);
            RasterDrawBrush = new SolidColorBrush(d2dDeviceContext, new Color4(0.5f, 0.5f, 0.5f, 1));
        }

        void Resize()
        {
            d2dDeviceContext.Target = null;
            d2dSurface.Dispose();
            swapChain.ResizeBuffers(0, 0, 0, Format.Unknown, SwapChainFlags.None);
            Surface1 dxgiSurface = swapChain.GetBackBuffer<Surface1>(0);
            var props = new BitmapProperties1(new PixelFormat(Format.B8G8R8A8_UNorm, D2D.AlphaMode.Ignore),
                d2dDeviceContext.DotsPerInch.Width,
                d2dDeviceContext.DotsPerInch.Height,
                BitmapOptions.Target | BitmapOptions.CannotDraw);
            d2dSurface = new Bitmap1(d2dDeviceContext, dxgiSurface, props);
            dxgiSurface.Dispose();
            d2dDeviceContext.Target = d2dSurface;
        }

        void Render()
        {
            if (resized)
            {
                Resize();
                resized = false;
            }

            d2dDeviceContext.BeginDraw();
            d2dDeviceContext.Clear(Color4.White);

            d2dDeviceContext.PushAxisAlignedClip(new RectangleF(5, 5, Child.Width - 10, Child.Height - 10), AntialiasMode.Aliased);
            d2dDeviceContext.Transform = Matrix3x2.Translation(Child.Width / 2f / Scale + Translation.X, Child.Height / 2f / Scale + Translation.Y) * Matrix3x2.Scaling(Scale);

            d2dDeviceContext.AntialiasMode = AntialiasMode.Aliased;
            d2dDeviceContext.DrawLine(new Vector2(0, int.MinValue), new Vector2(0, int.MaxValue), RasterDrawBrush, 1 / Scale);
            d2dDeviceContext.DrawLine(new Vector2(int.MinValue, 0), new Vector2(int.MaxValue, 0), RasterDrawBrush, 1 / Scale);
            d2dDeviceContext.AntialiasMode = AntialiasMode.PerPrimitive;

            RenderGeometry();

            d2dDeviceContext.Transform = Matrix3x2.Identity;
            d2dDeviceContext.PopAxisAlignedClip();

            d2dDeviceContext.EndDraw();
            swapChain.Present(0, PresentFlags.None);
        }

        void RenderGeometry()
        {
            var ellipse = new Ellipse(new Vector2(100, 100), 3, 3);
            d2dDeviceContext.FillEllipse(ellipse, VertexFillBrush);
            d2dDeviceContext.DrawEllipse(ellipse, VertexDrawBrush, 0.5f);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            VertexFillBrush.Dispose();
            VertexDrawBrush.Dispose();
            EdgeDrawBrush.Dispose();
            RasterDrawBrush.Dispose();

            d2dSurface.Dispose();
            d2dDeviceContext.Dispose();
            d2dDevice.Dispose();
            d2dFactory.Dispose();
            swapChain.Dispose();
            dxgiDevice.Dispose();
            d3dDevice.Dispose();
        }

        const uint SWP_NOZORDER = 0x0004;
        const uint SWP_NOACTIVATE = 0x0010;
        const uint SWP_ASYNCWINDOWPOS = 0x4000;

        [DllImport("user32.dll")]
        extern static bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        protected override void OnWindowPositionChanged(Rect rcBoundingBox)
        {
            if (Handle != IntPtr.Zero)
            {
                SetWindowPos(Handle,
                    IntPtr.Zero,
                    (int)rcBoundingBox.X,
                    (int)rcBoundingBox.Y,
                    (int)rcBoundingBox.Width,
                    (int)rcBoundingBox.Height,
                    SWP_ASYNCWINDOWPOS
                    | SWP_NOZORDER
                    | SWP_NOACTIVATE);
            }
        }
    }
}
