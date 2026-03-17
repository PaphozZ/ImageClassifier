using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ImageClassifier.ViewModel.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        int _count = 0;

        [ObservableProperty]
        private string _buttonText = "Click me";

        [RelayCommand]
        private void CounterClicked()
        {
            _count++;

            if (_count == 1)
                ButtonText = $"Clicked {_count} time";
            else
                ButtonText = $"Clicked {_count} times";
        }
    }
}
