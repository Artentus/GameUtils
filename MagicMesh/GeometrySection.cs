using System.ComponentModel;

namespace MagicMesh
{
    class GeometrySection : NotifyPropertyChangedBase
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
    }
}
