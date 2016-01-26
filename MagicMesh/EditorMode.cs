using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using SharpDX;

namespace MagicMesh
{
    abstract partial class EditorMode : NotifyPropertyChangedBase
    {
        static EditorMode cursor, line, arc, bezier, spline;

        public static EditorMode Cursor
        {
            get { return cursor ?? (cursor = new CursorMode()); }
        }

        public static EditorMode Line
        {
            get { return line ?? (line = new LineMode()); }
        }

        public static EditorMode Arc
        {
            get { return arc ?? (arc = new ArcMode()); }
        }

        public static EditorMode Bezier
        {
            get { return bezier ?? (bezier = new BezierMode()); }
        }

        public static EditorMode Spline
        {
            get { return spline ?? (spline = new SplineMode()); }
        }

        string name;
        string description;
        ImageSource image;

        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    name = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Name"));
                }
            }
        }

        public string Description
        {
            get { return description; }
            set
            {
                if (value != description)
                {
                    description = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Description"));
                }
            }
        }

        public ImageSource Image
        {
            get { return image; }
            set
            {
                if (value != image)
                {
                    image = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Image"));
                }
            }
        }

        public abstract void MouseClicked(Vector2 location);

        public abstract void MouseDragging(Vector2 newLocation);
    }
}
