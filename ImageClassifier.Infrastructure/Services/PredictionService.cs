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
        private readonly IDialogService _dialogService;
        private readonly IImageResizeService _imageResizeService;
        private readonly IModelManagerService _modelManager;

        private readonly ConcurrentBag<ImageData> _imagesData = new();

        public PredictionService(ITaskCommanderService taskCommanderService,
            IDialogService mauiDialogService,
            IImageResizeService imageResizeService,
            IModelManagerService modelManager)
        {
            _taskCommanderService = taskCommanderService;
            _dialogService = mauiDialogService;
            _imageResizeService = imageResizeService;
            _modelManager = modelManager;
        }

        private async Task PrepareImagesDataAsync(IEnumerable<ImageItemModel> items)
        {
            foreach (var item in items)
            {
                _taskCommanderService.AddTask(async () =>
                {
                    var bytes = await _imageResizeService.ResizeTo224(item.FullPath);
                    if (bytes != null)
                        _imagesData.Add(new ImageData { ImageBytes = bytes, FullPath = item.FullPath });
                });
            }
            await _taskCommanderService.WaitForAllAsync();
        }

        public async Task<IEnumerable<ImageItemModel>> ApplyPredictionsAsync(IEnumerable<ImageItemModel> items, string label)
        {
            MLContext mlContext = new();
            ITransformer trainedModel = null!;

            var modelData = await _modelManager.LoadModelDataAsync(label);
            if (modelData == null)
            {
                await _dialogService.DisplayAlert("Сообщение", "Модели с указанной меткой не обнаружено!", "OK");
                return Enumerable.Empty<ImageItemModel>();
            }
            if (_taskCommanderService.IsProcessing)
            {
                await _dialogService.DisplayAlert("Сообщение", "Классификация начнется после завершения фоновых задач", "OK");
            }
            _taskCommanderService.AddTask(() => Task.Run(() =>
            {
                using (var stream = new MemoryStream(modelData))
                {
                    trainedModel = mlContext.Model.Load(stream, out _);
                }
            }), true);
            await PrepareImagesDataAsync(items);

            var labeledItems = await PredictBatchAsync(mlContext, trainedModel, items);

            _imagesData.Clear();
            await _dialogService.DisplayAlert("Сообщение", "Классификация изображений выполнена!", "OK");
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
                        if (item != null && data.FullPath != null && predictedLabel != "Negative" && !string.IsNullOrEmpty(predictedLabel))
                        {
                            item.Labels.Add(new LabelModel(
                                name: predictedLabel!,
                                probability: probability,
                                lastModified: DateTime.Now));
                            results.Add(item);
                        }
                    }
                    return results;
                });
        }
    }
}
