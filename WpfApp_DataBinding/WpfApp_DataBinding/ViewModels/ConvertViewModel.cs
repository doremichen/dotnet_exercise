using System.ComponentModel;

namespace WpfApp_DataBinding.ViewModels
{
    public class ConvertViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged(nameof(IsChecked));
                }
            }
        }

        private void OnPropertyChanged(string v)
        {
            // NotifyPropertyChanged event is raised to inform the UI about the property change
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v));
        }
    }
}
