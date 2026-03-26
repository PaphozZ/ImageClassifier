using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageClassifier.Core.Enums;
using ImageClassifier.Core.Interfaces;
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
    private string? _predictButtonText = "";

    [ObservableProperty]
    private bool _isTrainMode;
    [ObservableProperty]
    private bool _isPreviewMode = true;

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
        if (file.FilePreview != null)
            _taskCommanderService.AddTask(() => Fullscreen.ShowImageAsync(file), true);
    }

    [RelayCommand]
    private void FullscreenTapped() => Fullscreen.Hide();

    [RelayCommand]
    private void DragStarting(ImageItemViewModel item)
    {
        _draggedItem = item;
    }

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
            }
            _draggedItem = null;
        }
    }

    [RelayCommand]
    private void TrainMode(ImageItemViewModel file)
    {
        IsTrainMode = !IsTrainMode;
        IsPreviewMode = !IsPreviewMode;

        if (IsTrainMode)
        {
            TrainButtonText = "Назад";
            PredictButtonText = "Обучить!";
        }
        else
        {
            PositiveItems.Clear();
            NegativeItems.Clear();
            _taskCommanderService.AddTask(FileCollection.ResetDatasetClasses, true);
            TrainButtonText = "Обучение";
            PredictButtonText = "";
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
        }
    }

    [RelayCommand]
    private async Task Train()
    {
        if (IsTrainMode)
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

    [RelayCommand]
    private async Task Predict()
    {
        if (IsTrainMode)
        {
            var positiveModels = PositiveItems.Select(f => f.ToModel());
            var labeledItems = await _predictionService.ApplyPredictionsAsync(positiveModels);
            await FileCollection.FillLabelsAsync(labeledItems);
        }
    }
}