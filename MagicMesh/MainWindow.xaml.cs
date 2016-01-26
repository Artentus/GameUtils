using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Forms;

namespace MagicMesh
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        public event PropertyChangedEventHandler PropertyChanged;

        readonly IntPtr handle;

        double oldX, oldY;
        double oldWidth, oldHeight;

        bool maximized;
        public bool Maximized
        {
            get { return maximized; }
            set
            {
                if (value != maximized)
                {
                    maximized = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Maximized"));
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            var interopHelper = new WindowInteropHelper(this);
            interopHelper.EnsureHandle();
            handle = interopHelper.Handle;
        }

        void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        void CanExecuteCommandDefault(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        void TitleBarMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (!Maximized) this.DragMove();
        }

        void TitelBarDoubleClickHandler(object sender, EventArgs e)
        {
            Maximize();
        }

        void WindowResizeHandler(object sender, MouseButtonEventArgs e)
        {
            SendMessage(handle, 0x112, (IntPtr)61448, IntPtr.Zero);
        }

        void Close(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
            e.Handled = true;
        }

        void Maximize()
        {
            if (Maximized)
            {
                Maximized = false;
                MaximizeItem.Icon = Resources["MaximizeMenuIcon"];
                MaximizeItem.Header = "Maximize";

                this.Left = oldX;
                this.Top = oldY;
                this.Width = oldWidth;
                this.Height = oldHeight;
            }
            else
            {
                Maximized = true;
                MaximizeItem.Icon = Resources["RestoreMenuIcon"];
                MaximizeItem.Header = "Restore";

                oldX = this.Left;
                oldY = this.Top;
                oldWidth = this.Width;
                oldHeight = this.Height;

                var currentScreen = Screen.FromHandle(handle);
                this.Left = currentScreen.WorkingArea.Left;
                this.Top = currentScreen.WorkingArea.Top;
                this.Width = currentScreen.WorkingArea.Width;
                this.Height = currentScreen.WorkingArea.Height;
            }
        }

        void Maximize(object sender, ExecutedRoutedEventArgs e)
        {
            Maximize();
            e.Handled = true;
        }

        void Minimize()
        {
            this.WindowState = WindowState.Minimized;
        }

        void Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            Minimize();
            e.Handled = true;
        }
    }
}
