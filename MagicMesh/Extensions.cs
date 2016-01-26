using System.Windows;

namespace MagicMesh
{
    static class Extensions
    {
        public static readonly DependencyProperty ShowCloseButtons =
            DependencyProperty.RegisterAttached("ShowCloseButtons",
                typeof(bool),
                typeof(Extensions),
                new PropertyMetadata(default(bool)));

        public static bool GetShowCloseButtons(UIElement element)
        {
            return (bool)element.GetValue(ShowCloseButtons);
        }

        public static void SetShowCloseButtons(UIElement element, bool value)
        {
            element.SetValue(ShowCloseButtons, value);
        }
    }
}
