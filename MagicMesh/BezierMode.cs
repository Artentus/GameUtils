using System;
using System.Windows.Media.Imaging;

namespace MagicMesh
{
    partial class EditorMode
    {
        sealed class BezierMode : EditorMode
        {
            public BezierMode()
            {
                Name = "Bezier tool";
                Description = "Draw bezier curves.";
                Image = new BitmapImage(new Uri(@"\Images\Bezier tool.jpg", UriKind.Relative));
            }
        }
    }
}
