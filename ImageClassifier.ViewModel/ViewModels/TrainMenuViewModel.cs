using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageClassifier.ViewModel.Enums;

namespace ImageClassifier.ViewModel.ViewModels
{
    public partial class TrainMenuViewModel : ObservableObject
    {
        private readonly ModeManagerViewModel _modeManagerViewModel;

        [ObservableProperty]
        private bool _trainMenuIsVisible;
        [ObservableProperty]
        private bool _acceptButtonIsEnabled;

        private string _newLabel = string.Empty;

        public TrainMenuViewModel(
            ModeManagerViewModel modeManagerViewModel)
        {
            _modeManagerViewModel = modeManagerViewModel;
        }

        public string NewLabel
        {
            get => _newLabel;
            set
            {
                if (_newLabel != value)
                {
                    _newLabel = value;
                    AcceptButtonIsEnabled = !string.IsNullOrEmpty(_newLabel);
                    OnPropertyChanged(nameof(NewLabel));
                }
            }
        }

        public void Show()
        {
            TrainMenuIsVisible = true;
        }

        [RelayCommand]
        public void Accept()
        {
            TrainMenuIsVisible = false;
            _modeManagerViewModel.SelectMode(AppMode.Train);
        }

        [RelayCommand]
        public void Cancel()
        {
            TrainMenuIsVisible = false;
        }
    }
}
