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
    private readonly IDialogService _dialogService;
    private readonly IImageResizeService _imageResizeService;
    private readonly IModelManagerService _modelManager;

    private readonly ConcurrentBag<ImageData> _imagesData = new();
    private readonly string _workSpacePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "model_workspace");

    public ModelTrainingService(
        ITaskCommanderService taskCommanderService, 
        IDialogService dialogService, 
        IImageResizeService imageResizeService,
        IModelManagerService modelManager)
    {
        _taskCommanderService = taskCommanderService;
        _dialogService = dialogService;
        _imageResizeService = imageResizeService;
        _modelManager = modelManager;
    }

    private async Task PrepareImagesDataAsync(IEnumerable<ImageItemModel> positiveItems, IEnumerable<ImageItemModel> negativeItems, string label)
    {
        foreach (var item in positiveItems)
        {
            var fileInfo = new FileInfo(item.FullPath);
            if (fileInfo.Exists && fileInfo.Length > 0)
            {
                _imagesData.Add(new ImageData { ImagePath = item.FullPath, Label = label });
            }
        }
        foreach (var item in negativeItems)
        {
            var fileInfo = new FileInfo(item.FullPath);
            if (fileInfo.Exists && fileInfo.Length > 0)
            {
                _imagesData.Add(new ImageData { ImagePath = item.FullPath, Label = "Negative" });
            }
        }

        await _taskCommanderService.WaitForAllAsync();
    }

    public async Task TrainAsync(IEnumerable<ImageItemModel> positiveItems, IEnumerable<ImageItemModel> negativeItems, string label)
    {
        MLContext mlContext = new();
        ITransformer trainedModel = null!;

        if (_taskCommanderService.IsProcessing) 
        {
            await _dialogService.DisplayAlert("Сообщение", "Обучение начнется после завершения фоновых задач", "OK");
        }
        var modelData = await _modelManager.LoadModelDataAsync(label);
        if (modelData != null)
        {
            _taskCommanderService.AddTask(() => Task.Run(() =>
            {
                using (var stream = new MemoryStream(modelData))
                {
                    trainedModel = mlContext.Model.Load(stream, out _);
                }
            }), true);
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
            .Append(mlContext.Transforms.LoadRawImageBytes(
                outputColumnName: "ImageBytes",
                imageFolder: null,
                inputColumnName: "ImagePath"
                ))
            .Append(mlContext.MulticlassClassification.Trainers.ImageClassification(options))
            .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        await Task.Run(() => trainedModel = pipeline.Fit(data));
        using var ms = new MemoryStream();
        mlContext.Model.Save(trainedModel, data.Schema, ms);

        _imagesData.Clear();
        await _modelManager.SaveModelAsync(new(label), ms.ToArray());
        await _dialogService.DisplayAlert("Сообщение", "Модель обучена и сохранена!", "OK");
    }
}