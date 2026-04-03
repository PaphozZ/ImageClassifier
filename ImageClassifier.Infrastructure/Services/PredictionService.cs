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
        private readonly IModelManagerService _modelManager;

        private readonly ConcurrentBag<ImageData> _imagesData = new();

        public PredictionService(ITaskCommanderService taskCommanderService,
            IDialogService mauiDialogService,
            IModelManagerService modelManager)
        {
            _taskCommanderService = taskCommanderService;
            _dialogService = mauiDialogService;
            _modelManager = modelManager;
        }

        private void PrepareImagesData(IEnumerable<ImageItemModel> items)
        {
            foreach (var item in items)
            {
                var fileInfo = new FileInfo(item.FullPath);
                if (fileInfo.Exists && fileInfo.Length > 0)
                {
                    _imagesData.Add(new ImageData { ImagePath = item.FullPath });
                }
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
            PrepareImagesData(items);

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
                        var item = items.FirstOrDefault(i => i.FullPath == data.ImagePath);
                        var model = await _modelManager.GetModelByLabelAsync(label);
                        var existingLabel = item?.Labels.FirstOrDefault(l => l.Name == label);
                        if (item != null
                            && model != null
                            && existingLabel != null
                            && model.LastModified < existingLabel.LastModified)
                        {
                            results.Add(item);
                            continue;
                        }

                        var prediction = predictionEngine.Predict(data);
                        var predictedLabel = prediction.PredictedLabel;
                        var probability = prediction.Score?.Max() ?? 0;
                        if (predictedLabel == "Negative")
                        {
                            predictedLabel = label;
                            probability = prediction.Score?.Min() ?? 0;
                        }
                        if (item != null
                            && data.ImagePath != null
                            && !string.IsNullOrEmpty(predictedLabel))
                        {
                            var newLabel = new LabelModel(
                                name: predictedLabel!,
                                probability: probability,
                                lastModified: DateTime.Now);
                            var oldLabel = item.Labels.FirstOrDefault(l => l.Name == predictedLabel);
                            if (oldLabel != null)
                                item.Labels.Remove(oldLabel);
                            item.Labels.Add(newLabel);
                            results.Add(item);
                        }
                    }
                    return results;
                });
        }
    }
}
