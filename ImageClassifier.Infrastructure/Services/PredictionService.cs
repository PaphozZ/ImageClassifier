using ImageClassifier.Core.Interfaces;
using ImageClassifier.Core.Models;
using ImageClassifier.Infrastructure.DTOs;
using Microsoft.ML;
using System.Collections.Concurrent;

namespace ImageClassifier.Infrastructure.Services
{
    public class PredictionService : IPredictionService
    {
        private readonly ITaskCommanderService _taskCommanderService;
        private readonly IDialogService _mauiDialogService;
        private readonly IImageResizeService _imageResizeService;

        private readonly ConcurrentBag<ImageData> _imagesData = new();
        private readonly string _workSpacePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "model_workspace");

        public PredictionService(ITaskCommanderService taskCommanderService,
            IDialogService mauiDialogService,
            IImageResizeService imageResizeService)
        {
            _taskCommanderService = taskCommanderService;
            _mauiDialogService = mauiDialogService;
            _imageResizeService = imageResizeService;
        }

        private async Task PrepareImagesDataAsync(IEnumerable<ImageItemModel> items)
        {
            foreach (var item in items)
            {
                _taskCommanderService.AddTask(async () =>
                {
                    var bytes = await _imageResizeService.ResizeTo224(item.FullPath);
                    if (bytes != null)
                        _imagesData.Add(new ImageData { ImageBytes = bytes, Label = "Positive", FullPath = item.FullPath });
                });
            }
            await _taskCommanderService.WaitForAllAsync();
        }

        public async Task<IEnumerable<ImageItemModel>> ApplyPredictionsAsync(IEnumerable<ImageItemModel> items)
        {
            MLContext mlContext = new();
            ITransformer trainedModel = null!;

            var modelPath = Path.Combine(_workSpacePath, "model.zip");
            if (!File.Exists(modelPath) || !items.Any())
            {
                await _mauiDialogService.DisplayAlert("Сообщение", "Сначала обучите модель и выберите изображения!", "OK");
                return Enumerable.Empty<ImageItemModel>();
            }
            if (_taskCommanderService.IsProcessing)
            {
                await _mauiDialogService.DisplayAlert("Сообщение", "Классификация начнется после завершения фоновых задач", "OK");
            }

            _taskCommanderService.AddTask(() => Task.Run(() =>
            {
                using var stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                trainedModel = mlContext.Model.Load(stream, out _);
            }), true);
            await PrepareImagesDataAsync(items);

            var labeledItems = await PredictBatchAsync(mlContext, trainedModel, items);

            _imagesData.Clear();
            await _mauiDialogService.DisplayAlert("Сообщение", "Классификация изображений выполнена!", "OK");
            return labeledItems;
        }

        private async Task<IEnumerable<ImageItemModel>> PredictBatchAsync(MLContext mlContext, ITransformer model, IEnumerable<ImageItemModel> items)
        {
            return await Task.Run(() =>
                {
                    var predictionEngine = mlContext.Model.CreatePredictionEngine<ImageData, ModelOutput>(model);
                    var results = new List<ImageItemModel>();

                    foreach (var data in _imagesData)
                    {
                        var prediction = predictionEngine.Predict(new ImageData { ImageBytes = data.ImageBytes });
                        var predictedLabel = prediction.PredictedLabel;
                        var probability = prediction.Score?.Max() ?? 0;

                        var item = items.FirstOrDefault(i => i.FullPath == data.FullPath);
                        if (item != null && data.FullPath != null && predictedLabel != "Negative")
                        {
                            item.Labels.Add(new LabelModel(
                                name: predictedLabel!,
                                probability: probability,
                                modelId: Guid.NewGuid(),
                                lastModified: DateTime.Now));
                            results.Add(item);
                        }
                    }
                    return results;
                });
        }
    }
}
