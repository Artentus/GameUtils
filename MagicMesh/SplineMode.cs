using System;
using System.Windows.Media.Imaging;

namespace MagicMesh
{
    partial class EditorMode
    {
        sealed class SplineMode : EditorMode
        {
            public SplineMode()
            {
                Name = "Spline tool";
                Description = "Draw cardinal spline curves";
                Image = new BitmapImage(new Uri(@"\Images\Spline tool.jpg", UriKind.Relative));
            }
        }
    }
}
