using CommunityToolkit.Mvvm.ComponentModel;
using ImageClassifier.ViewModel.Enums;

namespace ImageClassifier.ViewModel.ViewModels
{
    public partial class ModeManagerViewModel : ObservableObject
    {
        private readonly FileCollectionViewModel _fileCollection;

        [ObservableProperty]
        private AppMode _currentMode = AppMode.Preview;

        [ObservableProperty]
        private string? _trainButtonText = "Обучение";
        [ObservableProperty]
        private string? _predictButtonText = "Классификация";

        [ObservableProperty]
        private bool _trainButtonIsEnabled = true;
        [ObservableProperty]
        private bool _predictButtonIsEnabled = true;

        [ObservableProperty]
        private bool _isPreviewMode = true;

        [ObservableProperty]
        private bool _isPositiveVisible;
        [ObservableProperty]
        private bool _isNegativeVisible = true;

        public ModeManagerViewModel(
            FileCollectionViewModel fileCollection)
        {
            _fileCollection = fileCollection;
        }

        public void SelectMode(AppMode mode)
        {
            if (CurrentMode == mode) return;

            (CurrentMode, IsPreviewMode, IsPositiveVisible, IsNegativeVisible,
             TrainButtonText, PredictButtonText, TrainButtonIsEnabled, PredictButtonIsEnabled) = mode switch
             {
                 AppMode.Preview => (AppMode.Preview, true, false, true, "Обучение", "Классификация", true, true),
                 AppMode.Train => (AppMode.Train, false, true, true, "Назад", "Обучить!", true, true),
                 AppMode.Predict => (AppMode.Predict, false, true, true, "Классифицировать!", "Назад", true, true),
                 AppMode.Processing => (AppMode.Processing,
                                            false,
                                            IsPositiveVisible,
                                            IsNegativeVisible,
                                            TrainButtonText,
                                            PredictButtonText,
                                            false,
                                            false),
                 _ => default
             };

            if (CurrentMode == AppMode.Preview)
            {
                _fileCollection.ClearZones();
                _fileCollection.ResetDatasetClasses();
            }
        }
    }
}
