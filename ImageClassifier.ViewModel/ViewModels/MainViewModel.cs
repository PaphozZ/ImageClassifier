using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ImageClassifier.ViewModel.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        int _count = 0;
        public ICommand OnCounterClickedCommand { get; }

        private string _buttonText = "Click me";

        public MainViewModel()
        {
            OnCounterClickedCommand = new Command(OnCounterClicked);
        }

        public string ButtonText
        {  
            get => _buttonText;
            set 
            {
                _buttonText = value;
                OnPropertyChanged(nameof(ButtonText));
            }
        }

        public void OnCounterClicked()
        {
            _count++;

            if (_count == 1)
                ButtonText = $"Clicked {_count} time";
            else
                ButtonText = $"Clicked {_count} times";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
