using System;
using System.Windows.Media.Imaging;

namespace MagicMesh
{
    partial class EditorMode
    {
        sealed class ArcMode : EditorMode
        {
            public ArcMode()
            {
                Name = "Arc tool";
                Description = "Draw circular arcs.";
                Image = new BitmapImage(new Uri(@"\Images\Arc tool.jpg", UriKind.Relative));
            }
        }
    }
}
