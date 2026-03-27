using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageClassifier.Core.Enums;
using ImageClassifier.Core.Interfaces;
using ImageClassifier.Core.Models;
using ImageClassifier.ViewModel.Enums;
using System.Collections.ObjectModel;
using System.Runtime.Versioning;

namespace ImageClassifier.ViewModel.ViewModels;

public partial class PreviewViewModel : ObservableObject
{
    public FileCollectionViewModel FileCollection { get; }
    public FullscreenViewModel Fullscreen { get; }

    private readonly IFolderPicker _folderPicker;
    private readonly IDialogService _dialogService;
    private readonly IMediaPickerService _mediaPickerService;
    private readonly ITaskCommanderService _taskCommanderService;
    private readonly IModelTrainingService _modelTrainingService;
    private readonly IPredictionService _predictionService;

    private ImageItemViewModel? _draggedItem;

    [ObservableProperty]
    private string? _trainButtonText = "Обучение";
    [ObservableProperty]
    private string? _predictButtonText = "Классификация";

    private bool _isTrainMode;
    private bool _isPredictMode;

    private AppMode _currentMode = AppMode.Preview;

    [ObservableProperty]
    private bool _isPreviewMode = true;

    [ObservableProperty]
    private bool _isPositiveVisible;
    [ObservableProperty]
    private bool _isNegativeVisible = true;

    [ObservableProperty]
    private ImageItemViewModel? _selectedImageItem;

    [ObservableProperty]
    private ObservableCollection<ImageItemViewModel> _positiveItems = new();
    [ObservableProperty]
    private ObservableCollection<ImageItemViewModel> _negativeItems = new();

    public PreviewViewModel(
        FileCollectionViewModel fileCollection,
        FullscreenViewModel fullscreen,
        IFolderPicker folderPicker,
        IDialogService dialogService,
        IMediaPickerService mediaPickerService,
        ITaskCommanderService taskCommander,
        IModelTrainingService modelTrainingService,
        IPredictionService predictionService)
    {
        FileCollection = fileCollection;
        Fullscreen = fullscreen;

        _folderPicker = folderPicker;
        _dialogService = dialogService;
        _mediaPickerService = mediaPickerService;
        _taskCommanderService = taskCommander;
        _modelTrainingService = modelTrainingService;
        _predictionService = predictionService;

        _taskCommanderService.AddTask(FileCollection.LoadSavedFilesAsync);
    }

    [RelayCommand]
    private async Task PickImageAsync()
    {
        try
        {
            var model = await _mediaPickerService.PickImageAsync();
            if (model != null)
            {
                await FileCollection.AddFileAsync(model);
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
                await FileCollection.AddFilesFromFolderAsync(result.Folder.Path);
            }
        }
        catch
        {
            await _dialogService.DisplayAlert("Ошибка", "Не удалось загрузить файлы", "OK");
        }
    }

    [RelayCommand]
    private void SelectFile(ImageItemViewModel file)
    {
        SelectedImageItem = file;
        if (file.FilePreview != null)
            _taskCommanderService.AddTask(() => Fullscreen.ShowImageAsync(file), true);
    }

    [RelayCommand]
    private void FullscreenTapped() => Fullscreen.Hide();

    [RelayCommand]
    private void DragStarting(ImageItemViewModel item) => _draggedItem = item;


    [RelayCommand]
    private void DropToPositiveItems()
    {
        if (_draggedItem != null && !_draggedItem.IsDeleted && !PositiveItems.Contains(_draggedItem))
        {
            PositiveItems.Add(_draggedItem);
            if (NegativeItems.Contains(_draggedItem))
                NegativeItems.Remove(_draggedItem);
            _draggedItem.DatasetClass = DatasetClass.Positive;
            _draggedItem = null;

            if (_isPredictMode)
            {
                IsNegativeVisible = false;
            }
        }
    }

    [RelayCommand]
    private async Task DropToNegativeItems()
    {
        if (_draggedItem != null && !_draggedItem.IsDeleted && !NegativeItems.Contains(_draggedItem))
        {
            if (IsPreviewMode)
            {
                await FileCollection.RemoveFileAsync(_draggedItem);
            }
            else
            {
                NegativeItems.Add(_draggedItem);
                if (PositiveItems.Contains(_draggedItem))
                    PositiveItems.Remove(_draggedItem);
                _draggedItem.DatasetClass = DatasetClass.Negative;

                if (_isPredictMode)
                {
                    IsPositiveVisible = false;
                }
            }
            _draggedItem = null;
        }
    }

    [RelayCommand]
    private async Task TrainTapped(ImageItemViewModel file)
    {
        switch (_currentMode)
        {
            case AppMode.Preview:
                SelectMode(AppMode.Train);
                break;
            case AppMode.Train:
                SelectMode(AppMode.Preview);
                break;
            case AppMode.Predict:
                await Predict();
                break;
        }
    }

    [RelayCommand]
    private async Task PredictTapped(ImageItemViewModel file)
    {
        switch (_currentMode)
        {
            case AppMode.Preview:
                SelectMode(AppMode.Predict);
                break;
            case AppMode.Predict:
                SelectMode(AppMode.Preview);
                break;
            case AppMode.Train:
                await Train();
                break;
        }
    }

    private void SelectMode(AppMode mode)
    {
        (_isTrainMode, _isPredictMode, IsPreviewMode, IsPositiveVisible, IsNegativeVisible,
         TrainButtonText, PredictButtonText, _currentMode) = mode switch
         {
             AppMode.Preview => (false, false, true, false, true, "Обучение", "Классификация", AppMode.Preview),
             AppMode.Train => (true, false, false, true, true, "Назад", "Обучить!", AppMode.Train),
             AppMode.Predict => (false, true, false, true, true, "Классифицировать!", "Назад", AppMode.Predict),
             _ => default
         };

        if (mode == AppMode.Preview)
        {
            PositiveItems.Clear();
            NegativeItems.Clear();
            _taskCommanderService.AddTask(FileCollection.ResetDatasetClasses, true);
        }
    }

    [RelayCommand]
    private void RemoveLastPositiveItem()
    {
        var lastItem = PositiveItems.LastOrDefault();
        if (lastItem != null)
        {
            lastItem.DatasetClass = DatasetClass.None;
            PositiveItems.Remove(lastItem);

            if (_isPredictMode && PositiveItems.Count == 0)
            {
                IsNegativeVisible = true;
            }
        }
    }

    [RelayCommand]
    private void RemoveLastNegativeItem()
    {
        var lastItem = NegativeItems.LastOrDefault();
        if (lastItem != null)
        {
            lastItem.DatasetClass = DatasetClass.None;
            NegativeItems.Remove(lastItem);

            if (_isPredictMode && NegativeItems.Count == 0)
            {
                IsPositiveVisible = true;
            }
        }
    }

    private async Task Train()
    {
        if (_isTrainMode)
        {
            if (PositiveItems.Count > 0 && NegativeItems.Count > 0)
            {
                var positiveModels = PositiveItems.Select(f => f.ToModel());
                var negativeModels = NegativeItems.Select(f => f.ToModel());
                await _modelTrainingService.TrainAsync(positiveModels, negativeModels);
            }
            else
            {
                await _dialogService.DisplayAlert("Ошибка", "Выборки не могут быть пусты", "OK");
            }
        }
    }

    private async Task Predict()
    {
        if (_isPredictMode)
        {
            List<ImageItemModel> classificationModels = new();
            if (PositiveItems.Count == 0)
            {
                classificationModels = FileCollection.Files
                    .Where(f => !NegativeItems.Any(n => n.FullPath == f.FullPath) && !f.IsDeleted)
                    .Select(f => f.ToModel())
                    .ToList();
            }
            else if (NegativeItems.Count == 0)
            {
                classificationModels = PositiveItems
                    .Select(f => f.ToModel())
                    .ToList();
            }
            var labeledModels = await _predictionService.ApplyPredictionsAsync(classificationModels);
            await FileCollection.FillLabelsAsync(labeledModels);
        }
    }
}