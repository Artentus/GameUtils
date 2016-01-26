using System;
using System.Windows.Media.Imaging;
using SharpDX;

namespace MagicMesh
{
    partial class EditorMode
    {
        sealed class CursorMode : EditorMode
        {
            public CursorMode()
            {
                Name = "Cursor";
                Description = "Select, move and rotate objects.";
                Image = new BitmapImage(new Uri(@"\Images\Cursor.jpg", UriKind.Relative));
            }

            public override void MouseClicked(Vector2 location)
            {
                throw new NotImplementedException();
            }

            public override void MouseDragging(Vector2 newLocation)
            {
                throw new NotImplementedException();
            }
        }
    }
}
