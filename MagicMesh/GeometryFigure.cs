using System.ComponentModel;

namespace MagicMesh
{
    sealed class GeometryFigure : NotifyPropertyChangedBase
    {
        string name;

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

        public BindingList<GeometrySection> Sections { get; private set; }

        public GeometryFigure()
        {
            Sections = new BindingList<GeometrySection>();
        }
    }
}
