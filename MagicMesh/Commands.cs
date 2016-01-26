using System.Windows.Input;

namespace MagicMesh
{
    static class Commands
    {
        static ICommand maximize;
        public static ICommand Maximize
        {
            get { return maximize ?? (maximize = new RoutedCommand()); }
        }

        static ICommand minimize;
        public static ICommand Minimize
        {
            get { return minimize ?? (minimize = new RoutedCommand()); }
        }
    }
}
