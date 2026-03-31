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

        private void PrepareImagesDataAsync(IEnumerable<ImageItemModel> items)
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
        }

        public async Task<IEnumerable<ImageItemModel>> ApplyPredictionsAsync(IEnumerable<ImageItemModel> items, IEnumerable<string> labels)
        {
            MLContext mlContext = new();
            ITransformer trainedModel = null!;

            if (_taskCommanderService.IsProcessing)
            {
                await _dialogService.DisplayAlert("Сообщение", "Классификация начнется после завершения фоновых задач", "OK");
            }
            PrepareImagesDataAsync(items);

            List<ImageItemModel> labeledItems = new();
            foreach (var label in labels)
            {
                var modelData = await _modelManager.LoadModelDataAsync(label);
                if (modelData == null)
                {
                    await _dialogService.DisplayAlert("Сообщение", $"Модели с меткой \"{label}\" не обнаружено!", "OK");
                    continue;
                }
                _taskCommanderService.AddTask(() => Task.Run(() =>
                {
                    using (var stream = new MemoryStream(modelData))
                    {
                        trainedModel = mlContext.Model.Load(stream, out _);
                    }
                }), true);
                await _taskCommanderService.WaitForAllAsync();

                var labeledBatch = await PredictBatchAsync(mlContext, trainedModel, items, label);
                foreach (var batchItem in labeledBatch) 
                {
                    var labeledItem = labeledItems.FirstOrDefault(i => i.FullPath == batchItem.FullPath);
                    if (labeledItem == null)
                    {
                        labeledItems.Add(batchItem);
                    }
                    else 
                    {
                        labeledItem.Labels.AddRange(batchItem.Labels);
                    }
                }
            }
            _imagesData.Clear();
            await _dialogService.DisplayAlert("Сообщение", "Классификация изображений выполнена!", "OK");
            return labeledItems;
        }

        private async Task<IEnumerable<ImageItemModel>> PredictBatchAsync(MLContext mlContext, ITransformer model, IEnumerable<ImageItemModel> items, string label)
        {
            return await Task.Run(async () =>
                {
                    var predictionEngine = mlContext.Model.CreatePredictionEngine<ImageData, ModelOutput>(model);
                    var results = new List<ImageItemModel>();

                    foreach (var data in _imagesData)
                    {
                        var prediction = predictionEngine.Predict(new ImageData { ImageBytes = data.ImageBytes });
                        var predictedLabel = prediction.PredictedLabel;
                        var probability = prediction.Score?.Max() ?? 0;
                        var model = await _modelManager.GetModelByLabelAsync(label);
                        var modifiedDate = model?.LastModified;

                        var item = items.FirstOrDefault(i => i.FullPath == data.FullPath);
                        if (item != null && data.FullPath != null && predictedLabel != "Negative" && !string.IsNullOrEmpty(predictedLabel))
                        {
                            item.Labels.Add(new LabelModel(
                                name: predictedLabel!,
                                probability: probability,
                                lastModified: modifiedDate ?? throw new InvalidOperationException()));
                            results.Add(item);
                        }
                    }
                    return results;
                });
        }
    }
}
