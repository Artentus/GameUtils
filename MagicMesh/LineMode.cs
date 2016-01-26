using System;
using System.Windows.Media.Imaging;

namespace MagicMesh
{
    partial class EditorMode
    {
        sealed class LineMode : EditorMode
        {
            public LineMode()
            {
                Name = "Line tool";
                Description = "Draw straight lines.";
                Image = new BitmapImage(new Uri(@"\Images\Line tool.jpg", UriKind.Relative));
            }
        }
    }
}
