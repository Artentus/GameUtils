using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace MagicMesh
{
    sealed class MainViewModel : NotifyPropertyChangedBase
    {
        static MainViewModel instance;

        public static MainViewModel Instance
        {
            get { return instance ?? (instance = new MainViewModel()); }
        }

        Project selectedProject;
        EditorMode currentEditorMode;

        public ICommand NewCommand { get; private set; }
        public ICommand OpenCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand SaveAsCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }
        public ICommand UndoCommand { get; private set; }
        public ICommand RedoCommand { get; private set; }

        public BindingList<Project> OpenProjects { get; private set; }

        public Project SelectedProject
        {
            get { return selectedProject; }
            set
            {
                if (value != selectedProject)
                {
                    selectedProject = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("SelectedProject"));
                }
            }
        }

        public BindingList<EditorMode> EditorModes { get; private set; }

        public EditorMode CurrentEditorMode
        {
            get { return currentEditorMode; }
            set
            {
                if (value != currentEditorMode)
                {
                    currentEditorMode = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("CurrentEditorMode"));
                }
            }
        }

        private MainViewModel()
        {
            NewCommand = new RelayCommand(CreateNew);
            OpenCommand = new RelayCommand(Open);
            SaveCommand = new RelayCommand(Save, CanSave);
            SaveAsCommand = new RelayCommand(SaveAs, CanSave);
            ExportCommand = new RelayCommand(Export, CanSave);
            UndoCommand = new RelayCommand(Undo, CanUndo);
            RedoCommand = new RelayCommand(Redo, CanRedo);

            OpenProjects = new BindingList<Project>();
            EditorModes = new BindingList<EditorMode>
            {
                EditorMode.Cursor,
                EditorMode.Line,
                EditorMode.Arc,
                EditorMode.Bezier,
                EditorMode.Spline,
            };
            currentEditorMode = EditorModes.First();
        }

        private void CreateNew()
        {
            var project = new Project();
            project.RemovingRequested += RemovingRequestedHandler;
            OpenProjects.Add(project);
            SelectedProject = project;
        }

        private void Open()
        {
            var ofd = new OpenFileDialog();
            ofd.DefaultExt = ".mmproj";
            ofd.Filter = "MagicMesh project files|*.mmproj|All files|*.*";
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.Multiselect = true;

            if (ofd.ShowDialog() == true)
            {
                Project project = null;
                foreach (string fileName in ofd.FileNames)
                {
                    project = Project.Load(fileName);
                    project.RemovingRequested += RemovingRequestedHandler;
                    OpenProjects.Add(project);
                }
                SelectedProject = project;
            }
        }

        private void RemovingRequestedHandler(object sender, EventArgs e)
        {
            Project project = (Project)sender;
            if (!project.IsSaved)
            {
                new ConfirmDialog().ShowDialog();

                //MessageBoxResult result =
                //    MessageBox.Show(
                //        "There are stil some unsaved changes in this project. Do you want to close it anyways?",
                //        "Close without saving?",
                //        MessageBoxButton.YesNo);

                //switch (result)
                //{
                //    case MessageBoxResult.Yes:
                //        break;
                //    default:
                //        return;
                //}
            }

            if (project == SelectedProject)
            {
                if (OpenProjects.Count == 1)
                {
                    SelectedProject = null;
                }
                else
                {
                    int newIndex = OpenProjects.IndexOf(project) - 1;
                    if (newIndex < 0)
                    {
                        newIndex += 2;
                        SelectedProject = newIndex < OpenProjects.Count ? OpenProjects[newIndex] : null;
                    }
                    SelectedProject = OpenProjects[newIndex];
                }
            }
            OpenProjects.Remove(project);
        }

        private bool CanSave()
        {
            return SelectedProject != null;
        }

        private void Save()
        {
            if (SelectedProject.WasNeverSaved)
                SaveAs();
            else
                SelectedProject.Save();
        }

        private void SaveAs()
        {
            
        }

        private void Export()
        {
            
        }

        private bool CanUndo()
        {
            return false;
        }

        private void Undo()
        {
            
        }

        private bool CanRedo()
        {
            return false;
        }

        private void Redo()
        {
            
        }
    }
}
