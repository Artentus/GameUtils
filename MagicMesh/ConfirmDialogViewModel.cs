using System.Windows.Input;

namespace MagicMesh
{
    class ConfirmDialogViewModel : NotifyPropertyChangedBase
    {
        public ICommand YesCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }

        public ConfirmResult Result { get; private set; }

        public ConfirmDialogViewModel()
        {
            Result = ConfirmResult.Cancel;

            YesCommand = new RelayCommand(() => Result = ConfirmResult.Yes);
            CancelCommand = new RelayCommand(() => Result = ConfirmResult.Cancel);
            SaveCommand = new RelayCommand(() => Result = ConfirmResult.Save);
        }
    }
}
