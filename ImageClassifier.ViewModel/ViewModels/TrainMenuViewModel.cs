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
        [ObservableProperty]
        private bool _entryIsEnabled = true;
        [ObservableProperty]
        private string _placeholderText = "Введите название метки";

        private string _newLabel = string.Empty;
        private string _selectedLabel = string.Empty;

        [ObservableProperty]
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
                    AcceptButtonIsEnabled = !string.IsNullOrEmpty(_newLabel) 
                        && _newLabel != "Новая метка"
                        && !Labels.Contains(_newLabel);
                    OnPropertyChanged(nameof(NewLabel));
                }
            }
        }

        public string SelectedLabel
        {
            get => _selectedLabel;
            set
            {
                if (_selectedLabel != value)
                {
                    NewLabel = string.Empty;
                    _selectedLabel = value;
                    EntryIsEnabled = _selectedLabel == "Новая метка";
                    PlaceholderText = _selectedLabel == "Новая метка" 
                        ? "Введите название метки" 
                        : "Метка будет дополнена";
                    AcceptButtonIsEnabled = !string.IsNullOrEmpty(_selectedLabel) 
                        && _selectedLabel != "Новая метка";
                    OnPropertyChanged(nameof(SelectedLabel));
                }
            }
        }

        public async Task Show()
        {
            NewLabel = string.Empty;
            Labels.Clear();
            var modelModels = await _modelManagerService.GetAllModelsAsync();
            foreach (var model in modelModels)
            {
                Labels.Add(model.LabelName);
            }
            Labels.Add("Новая метка");
            SelectedLabel = Labels.LastOrDefault()!;
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
