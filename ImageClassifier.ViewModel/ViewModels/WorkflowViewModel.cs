using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageClassifier.Core.Interfaces;
using ImageClassifier.Core.Models;
using ImageClassifier.ViewModel.Enums;
using ImageClassifier.ViewModel.ViewModels;

public partial class WorkflowViewModel : ObservableObject
{
    private readonly FileCollectionViewModel _fileCollection;

    private readonly IModelTrainingService _modelTrainingService;
    private readonly IPredictionService _predictionService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private AppMode _currentMode = AppMode.Preview;

    [ObservableProperty]
    private string? _trainButtonText = "Обучение";
    [ObservableProperty]
    private string? _predictButtonText = "Классификация";

    [ObservableProperty]
    private bool _isPreviewMode = true;

    [ObservableProperty]
    private bool _isPositiveVisible;
    [ObservableProperty]
    private bool _isNegativeVisible = true;

    public WorkflowViewModel(
        FileCollectionViewModel fileCollection,
        IModelTrainingService modelTrainingService,
        IPredictionService predictionService,
        IDialogService dialogService)
    {
        _fileCollection = fileCollection;
        _modelTrainingService = modelTrainingService;
        _predictionService = predictionService;
        _dialogService = dialogService;
    }

    public void SelectMode(AppMode mode)
    {
        if (CurrentMode == mode) return;

        (CurrentMode, IsPreviewMode, IsPositiveVisible, IsNegativeVisible,
         TrainButtonText, PredictButtonText) = mode switch
         {
             AppMode.Preview => (AppMode.Preview, true, false, true, "Обучение", "Классификация"),
             AppMode.Train => (AppMode.Train, false, true, true, "Назад", "Обучить!"),
             AppMode.Predict => (AppMode.Predict, false, true, true, "Классифицировать!", "Назад"),
             _ => default
         };
        _fileCollection.ClearZones();
        _fileCollection.ResetDatasetClasses();
    }

    [RelayCommand]
    private async Task TrainTapped(ImageItemViewModel file)
    {
        switch (CurrentMode)
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
        switch (CurrentMode)
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

    private async Task Train()
    {
        if (CurrentMode == AppMode.Train)
        {
            if (_fileCollection.PositiveItems.Count > 0 && _fileCollection.NegativeItems.Count > 0)
            {
                var positiveModels = _fileCollection.PositiveItems.Select(f => f.ToModel());
                var negativeModels = _fileCollection.NegativeItems.Select(f => f.ToModel());
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
        if (CurrentMode == AppMode.Predict)
        {
            List<ImageItemModel> classificationModels = new();
            if (_fileCollection.PositiveItems.Count == 0)
            {
                classificationModels = _fileCollection.Files
                    .Where(f => !_fileCollection.NegativeItems.Any(n => n.FullPath == f.FullPath) && !f.IsDeleted)
                    .Select(f => f.ToModel())
                    .ToList();
            }
            else if (_fileCollection.NegativeItems.Count == 0)
            {
                classificationModels = _fileCollection.PositiveItems
                    .Select(f => f.ToModel())
                    .ToList();
            }
            var labeledModels = await _predictionService.ApplyPredictionsAsync(classificationModels);
            await _fileCollection.FillLabelsAsync(labeledModels);
        }
    }
}