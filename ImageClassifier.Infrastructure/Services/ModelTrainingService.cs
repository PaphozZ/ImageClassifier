using ImageClassifier.Core.Interfaces;
using ImageClassifier.Core.Models;
using ImageClassifier.Infrastructure.DTOs;
using Microsoft.ML;
using Microsoft.ML.Vision;
using SkiaSharp;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ImageClassifier.Infrastructure.Services;

public class ModelTrainingService : IModelTrainingService
{
    private readonly ITaskCommanderService _taskCommanderService;
    private readonly IDialogService _mauiDialogService;

    private readonly ConcurrentBag<ImageData> _imagesData = new();
    private readonly string _workSpacePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "model_workspace");

    private MLContext _mlContext = new MLContext();

    private ITransformer? _trainedModel;

    public ModelTrainingService(ITaskCommanderService taskCommanderService, IDialogService mauiDialogService)
    {
        _taskCommanderService = taskCommanderService;
        _mauiDialogService = mauiDialogService;
    }

    private async Task PrepareImagesDataAsync(IEnumerable<ImageItemModel> positiveItems, IEnumerable<ImageItemModel> negativeItems)
    {
        foreach (var item in positiveItems)
        {
            _taskCommanderService.AddTask(async() =>
            {
                var bytes = await ResizeTo224(item.FullPath);
                if (bytes != null)
                    _imagesData.Add(new ImageData { ImageBytes = bytes, Label = "Positive" });
            });
        }
        foreach (var item in negativeItems)
        {
            _taskCommanderService.AddTask(async() =>
            {
                var bytes = await ResizeTo224(item.FullPath);
                if (bytes != null)
                    _imagesData.Add(new ImageData { ImageBytes = bytes, Label = "Negative" });
            });
        }

        await _taskCommanderService.WaitForAllAsync();
    }

    private async Task<byte[]?> ResizeTo224(string imagePath)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var original = SKBitmap.Decode(imagePath);
                if (original == null) return null;

                using var resized = new SKBitmap(224, 224);
                original.ScalePixels(resized, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));

                using var image = SKImage.FromBitmap(resized);
                using var data = image.Encode(SKEncodedImageFormat.Png, 90);
                return data.ToArray();
            }
            catch
            {
                return null;
            }
        });
    }

    public async Task TrainAsync(IEnumerable<ImageItemModel> positiveItems, IEnumerable<ImageItemModel> negativeItems)
    {
        if (_taskCommanderService.IsProcessing) 
        {
            await _mauiDialogService.DisplayAlert("Сообщение", "Обучение начнется после завершения фоновых задач", "OK");
        }
        await PrepareImagesDataAsync(positiveItems, negativeItems);

        var options = new ImageClassificationTrainer.Options()
        {
            Arch = ImageClassificationTrainer.Architecture.ResnetV250,
            LabelColumnName = "Label",
            FeatureColumnName = "ImageBytes",
            MetricsCallback = (metrics) => Debug.WriteLine(metrics),
            WorkspacePath = _workSpacePath,
            ResourcePath = _workSpacePath //Модифицированная версия библиотеки
        };

        IDataView data = _mlContext.Data.LoadFromEnumerable(_imagesData);

        var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label")
            .Append(_mlContext.MulticlassClassification.Trainers.ImageClassification(options))
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        await Task.Run(() => _trainedModel = pipeline.Fit(data));
        _mlContext.Model.Save(_trainedModel, data.Schema, Path.Combine(_workSpacePath, "model.zip"));

        await _mauiDialogService.DisplayAlert("Сообщение", "Модель обучена и сохранена!", "OK");

        _imagesData.Clear();
        _mlContext = new MLContext();
        _trainedModel = null;
    }
}