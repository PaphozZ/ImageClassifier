using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageClassifier.Core.Interfaces;
using ImageClassifier.ViewModel.Enums;
using System.Collections.ObjectModel;

namespace ImageClassifier.ViewModel.ViewModels
{
    public partial class PredictMenuViewModel : ObservableObject
    {
        private readonly IModelManagerService _modelManagerService;
        private readonly ModeManagerViewModel _modeManagerViewModel;

        [ObservableProperty]
        private bool _PredictMenuIsVisible;
        [ObservableProperty]
        private bool _acceptButtonIsEnabled;

        [ObservableProperty]
        private ObservableCollection<string> _labels = new();

        private string _selectedLabel = string.Empty;

        public PredictMenuViewModel(
            ModeManagerViewModel modeManagerViewModel,
            IModelManagerService modelManagerService)
        {
            _modeManagerViewModel = modeManagerViewModel;
            _modelManagerService = modelManagerService;
        }

        public string SelectedLabel
        {
            get => _selectedLabel;
            set
            {
                if (_selectedLabel != value)
                {
                    _selectedLabel = value;
                    AcceptButtonIsEnabled = !string.IsNullOrEmpty(_selectedLabel);
                    OnPropertyChanged(nameof(SelectedLabel));
                }
            }
        }

        public async Task Show()
        {
            Labels.Clear();
            var modelModels = await _modelManagerService.GetAllModelsAsync();
            foreach (var model in modelModels)
            {
                Labels.Add(model.LabelName);
            }
            PredictMenuIsVisible = true;
        }

        [RelayCommand]
        public void Accept()
        {
            PredictMenuIsVisible = false;
            _modeManagerViewModel.SelectMode(AppMode.Predict);
        }

        [RelayCommand]
        public void Cancel()
        {
            PredictMenuIsVisible = false;
        }
    }
}
