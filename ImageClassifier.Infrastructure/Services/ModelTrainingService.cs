using ImageClassifier.Core.Interfaces;
using ImageClassifier.Core.Models;
using ImageClassifier.Infrastructure.DTOs;
using Microsoft.ML;
using Microsoft.ML.Vision;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ImageClassifier.Infrastructure.Services;

public class ModelTrainingService : IModelTrainingService
{
    private readonly ITaskCommanderService _taskCommanderService;
    private readonly IDialogService _mauiDialogService;
    private readonly IImageResizeService _imageResizeService;

    private readonly ConcurrentBag<ImageData> _imagesData = new();
    private readonly string _workSpacePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "model_workspace");

    public ModelTrainingService(
        ITaskCommanderService taskCommanderService, 
        IDialogService mauiDialogService, 
        IImageResizeService imageResizeService)
    {
        _taskCommanderService = taskCommanderService;
        _mauiDialogService = mauiDialogService;
        _imageResizeService = imageResizeService;
    }

    private async Task PrepareImagesDataAsync(IEnumerable<ImageItemModel> positiveItems, IEnumerable<ImageItemModel> negativeItems, string label)
    {
        foreach (var item in positiveItems)
        {
            _taskCommanderService.AddTask(async() =>
            {
                var bytes = await _imageResizeService.ResizeTo224(item.FullPath);
                if (bytes != null)
                    _imagesData.Add(new ImageData { ImageBytes = bytes, Label = label });
            });
        }
        foreach (var item in negativeItems)
        {
            _taskCommanderService.AddTask(async() =>
            {
                var bytes = await _imageResizeService.ResizeTo224(item.FullPath);
                if (bytes != null)
                    _imagesData.Add(new ImageData { ImageBytes = bytes, Label = "Negative" });
            });
        }

        await _taskCommanderService.WaitForAllAsync();
    }

    public async Task TrainAsync(IEnumerable<ImageItemModel> positiveItems, IEnumerable<ImageItemModel> negativeItems, string label)
    {
        MLContext mlContext = new();
        ITransformer trainedModel = null!;

        if (_taskCommanderService.IsProcessing) 
        {
            await _mauiDialogService.DisplayAlert("Сообщение", "Обучение начнется после завершения фоновых задач", "OK");
        }
        await PrepareImagesDataAsync(positiveItems, negativeItems, label);

        var options = new ImageClassificationTrainer.Options()
        {
            Arch = ImageClassificationTrainer.Architecture.ResnetV250,
            LabelColumnName = "Label",
            FeatureColumnName = "ImageBytes",
            MetricsCallback = (metrics) => Debug.WriteLine(metrics),
            WorkspacePath = _workSpacePath,
            ResourcePath = _workSpacePath   //модифицированная версия библиотеки
        };

        IDataView data = mlContext.Data.LoadFromEnumerable(_imagesData);

        var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Label")
            .Append(mlContext.MulticlassClassification.Trainers.ImageClassification(options))
            .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        await Task.Run(() => trainedModel = pipeline.Fit(data));
        mlContext.Model.Save(trainedModel, data.Schema, Path.Combine(_workSpacePath, "model.zip"));

        _imagesData.Clear();
        await _mauiDialogService.DisplayAlert("Сообщение", "Модель обучена и сохранена!", "OK");
    }
}