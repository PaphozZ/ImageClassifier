using CommunityToolkit.Mvvm.ComponentModel;
using ImageClassifier.Core.Interfaces;
using ImageClassifier.Core.Models;
using ImageClassifier.ViewModel.ViewModels;

public partial class WorkflowViewModel : ObservableObject
{
    private readonly FileCollectionViewModel _fileCollection;

    private readonly IModelTrainingService _modelTrainingService;
    private readonly IPredictionService _predictionService;
    private readonly IDialogService _dialogService;

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

    public async Task Train(string label)
    {
        if (_fileCollection.PositiveItems.Count > 0 && _fileCollection.NegativeItems.Count > 0)
        {
            var positiveModels = _fileCollection.PositiveItems.Select(f => f.ToModel());
            var negativeModels = _fileCollection.NegativeItems.Select(f => f.ToModel());
            await _modelTrainingService.TrainAsync(positiveModels, negativeModels, label);
        }
        else
        {
            await _dialogService.DisplayAlert("Ошибка", "Выборки не могут быть пусты", "OK");
        }
    }

    public async Task Predict()
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