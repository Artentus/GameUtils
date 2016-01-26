using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using SharpDX;

namespace MagicMesh
{
    sealed class Project : NotifyPropertyChangedBase
    {
        string name;
        bool isSaved;
        Vector2 translation;
        float scale;

        public event EventHandler RemovingRequested;

        public ICommand RemoveCommand { get; private set; }

        public string Name
        {
            get { return name; }
            private set
            {
                if (value != name)
                {
                    name = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Name"));
                }
            }
        }

        public BindingList<GeometryFigure> Figures { get; private set; }

        public bool WasNeverSaved { get; private set; }

        public bool IsSaved
        {
            get { return isSaved; }
            private set
            {
                if (value != isSaved)
                {
                    isSaved = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsSaved"));
                }
            }
        }

        public Vector2 Translation
        {
            get { return translation; }
            set
            {
                translation = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Translation"));
            }
        }

        public float Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Scale"));
            }
        }

        public Project()
        {
            Figures = new BindingList<GeometryFigure>();
            scale = 1f;
            WasNeverSaved = true;
            Name = "Untitled";
            RemoveCommand = new RelayCommand(OnRemovingRequested);
        }

        public static Project Load(string sourceFile)
        {
            return new Project(sourceFile);
        }

        private Project(string sourceFile)
        {
            var file = new FileInfo(sourceFile);

        }

        private void OnRemovingRequested()
        {
            if (RemovingRequested != null)
                RemovingRequested(this, EventArgs.Empty);
        }

        public void SaveAs(string destinationFile)
        {
            
        }

        public void Save()
        {
            IsSaved = true;
        }
    }
}
