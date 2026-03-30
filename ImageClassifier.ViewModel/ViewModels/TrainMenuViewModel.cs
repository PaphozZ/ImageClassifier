using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageClassifier.Core.Interfaces;
using ImageClassifier.ViewModel.Enums;
using System.Collections.ObjectModel;

namespace ImageClassifier.ViewModel.ViewModels
{
    public partial class TrainMenuViewModel : ObservableObject
    {
        private readonly IModelManagerService _modelManagerService;
        private readonly ModeManagerViewModel _modeManagerViewModel;

        [ObservableProperty]
        private bool _trainMenuIsVisible;
        [ObservableProperty]
        private bool _acceptButtonIsEnabled;

        private string _newLabel = string.Empty;
        private ObservableCollection<string> _labels = new();

        public TrainMenuViewModel(
            ModeManagerViewModel modeManagerViewModel,
            IModelManagerService modelManagerService)
        {
            _modeManagerViewModel = modeManagerViewModel;
            _modelManagerService = modelManagerService;
        }

        public string NewLabel
        {
            get => _newLabel;
            set
            {
                if (_newLabel != value)
                {
                    _newLabel = value;
                    AcceptButtonIsEnabled = !string.IsNullOrEmpty(_newLabel) && !_labels.Contains(_newLabel);
                    OnPropertyChanged(nameof(NewLabel));
                }
            }
        }

        public async Task Show()
        {
            NewLabel = string.Empty;
            _labels.Clear();
            var modelModels = await _modelManagerService.GetAllModelsAsync();
            foreach (var model in modelModels)
            {
                _labels.Add(model.LabelName);
            }
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
