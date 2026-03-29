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

        public TrainMenuViewModel(
            ModeManagerViewModel modeManagerViewModel)
        {
            _modeManagerViewModel = modeManagerViewModel;
        }

        [RelayCommand]
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
