using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageClassifier.Core.Enums;
using ImageClassifier.Core.Interfaces;
using ImageClassifier.ViewModel.Enums;
using System.Collections.ObjectModel;
using System.Runtime.Versioning;

namespace ImageClassifier.ViewModel.ViewModels
{
    public partial class SidePanelViewModel : ObservableObject
    {
        private readonly FileCollectionViewModel _fileCollection;
        private readonly WorkflowViewModel _workflow;
        private readonly TrainMenuViewModel _trainMenu;
        private readonly ModeManagerViewModel _modeManager;
        private readonly PredictMenuViewModel _predictMenu;

        [ObservableProperty]
        private ObservableCollection<LabelViewModel> _labelsCollection = new();

        private readonly IFolderPicker _folderPicker;
        private readonly IDialogService _dialogService;
        private readonly IMediaPickerService _mediaPickerService;
        private readonly IModelManagerService _modelManagerService;
        private readonly ITaskCommanderService _taskCommanderService;

        public SidePanelViewModel(
        FileCollectionViewModel fileCollection,
        WorkflowViewModel workflow,
        TrainMenuViewModel trainMenu,
        ModeManagerViewModel modeManager,
        IFolderPicker folderPicker,
        IDialogService dialogService,
        IMediaPickerService mediaPickerService,
        PredictMenuViewModel predictMenu,
        IModelManagerService modelManagerService,
        ITaskCommanderService taskCommanderService)
        {
            _fileCollection = fileCollection;
            _workflow = workflow;
            _trainMenu = trainMenu;
            _modeManager = modeManager;
            _predictMenu = predictMenu;

            _folderPicker = folderPicker;
            _dialogService = dialogService;
            _mediaPickerService = mediaPickerService;
            _modelManagerService = modelManagerService;
            _taskCommanderService = taskCommanderService;

            _taskCommanderService.AddTask(GetLabels, true);
        }

        private async Task GetLabels()
        {
            LabelsCollection.Clear();
            var models = await _modelManagerService.GetAllModelsAsync();
            foreach (var model in models)
            {
                LabelsCollection.Add(new LabelViewModel(model.LabelName, 1, DateTime.Now));
            }
        }

        [RelayCommand]
        private async Task PickImageAsync()
        {
            try
            {
                var model = await _mediaPickerService.PickImageAsync();
                if (model != null)
                {
                    await _fileCollection.AddFileAsync(model);
                }
            }
            catch
            {
                await _dialogService.DisplayAlert("Ошибка", "Не удалось загрузить файл", "OK");
            }
        }

        [RelayCommand]
        [SupportedOSPlatform("windows")]
        private async Task PickFolderAsync()
        {
            try
            {
                var result = await _folderPicker.PickAsync(default);
                if (result.IsSuccessful)
                {
                    await _fileCollection.AddFilesFromFolderAsync(result.Folder.Path);
                }
            }
            catch
            {
                await _dialogService.DisplayAlert("Ошибка", "Не удалось загрузить файлы", "OK");
            }
        }

        [RelayCommand]
        private async Task TrainTapped()
        {
            switch (_modeManager.CurrentMode)
            {
                case AppMode.Preview:
                    await _trainMenu.Show();
                    break;
                case AppMode.Train:
                    await GetLabels();
                    _modeManager.SelectMode(AppMode.Preview);
                    break;
                case AppMode.Predict:
                    _modeManager.SelectMode(AppMode.Processing);
                    var labels = _predictMenu.CheckBoxIsChecked
                        ? [.. _predictMenu.Labels]
                        : new[] { _predictMenu.SelectedLabel };
                    await _workflow.Predict(labels);
                    _modeManager.SelectMode(AppMode.Predict);
                    break;
            }
        }

        [RelayCommand]
        private async Task PredictTapped()
        {
            switch (_modeManager.CurrentMode)
            {
                case AppMode.Preview:
                    await _predictMenu.Show();
                    break;
                case AppMode.Predict:
                    await GetLabels();
                    _modeManager.SelectMode(AppMode.Preview);
                    break;
                case AppMode.Train:
                    _modeManager.SelectMode(AppMode.Processing);
                    var label = string.IsNullOrEmpty(_trainMenu.NewLabel)
                        ? _trainMenu.SelectedLabel
                        : _trainMenu.NewLabel;
                    await _workflow.Train(label);
                    _modeManager.SelectMode(AppMode.Train);
                    break;
            }
        }

        [RelayCommand]
        private void RemoveLastPositiveItem()
        {
            var lastItem = _fileCollection.PositiveItems.LastOrDefault();
            if (lastItem != null)
            {
                lastItem.DatasetClass = DatasetClass.None;
                _fileCollection.PositiveItems.Remove(lastItem);

                if (_modeManager.CurrentMode == AppMode.Predict && _fileCollection.PositiveItems.Count == 0)
                {
                    _modeManager.IsNegativeVisible = true;
                }
            }
        }

        [RelayCommand]
        private void RemoveLastNegativeItem()
        {
            var lastItem = _fileCollection.NegativeItems.LastOrDefault();
            if (lastItem != null)
            {
                lastItem.DatasetClass = DatasetClass.None;
                _fileCollection.NegativeItems.Remove(lastItem);

                if (_modeManager.CurrentMode == AppMode.Predict && _fileCollection.NegativeItems.Count == 0)
                {
                    _modeManager.IsPositiveVisible = true;
                }
            }
        }
    }
}
