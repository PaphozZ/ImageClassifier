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
                        _imagesData.Add(new ImageData { ImageBytes = bytes, Label = "Positive" });
                });
            }

            await _taskCommanderService.WaitForAllAsync();
        }

        public async Task ApplyPredictionsAsync(IEnumerable<ImageItemModel> items)
        {
            MLContext mlContext = new();
            ITransformer trainedModel = null!;

            using (var stream = new FileStream(Path.Combine(_workSpacePath, "model.zip"), FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                trainedModel = mlContext.Model.Load(stream, out var modelSchema);
            }
            if (trainedModel == null || items.Count() == 0)
            {
                await _mauiDialogService.DisplayAlert("Сообщение", "Сначала обучите модель и выберите изображения!", "OK");
                return;
            }
            if (_taskCommanderService.IsProcessing)
            {
                await _mauiDialogService.DisplayAlert("Сообщение", "Классификация начнется после завершения фоновых задач", "OK");
            }
            await PrepareImagesDataAsync(items);

            var predictionEngine = mlContext.Model.CreatePredictionEngine<ImageData, ModelOutput>(trainedModel);

            foreach (var item in _imagesData)
            {
                var prediction = predictionEngine.Predict(new ImageData { ImageBytes = item.ImageBytes });
                var resuslt = $"Результат: {prediction.PredictedLabel} (вероятность: {prediction.Score!.Max():P2})";
            }

            _imagesData.Clear();
            await _mauiDialogService.DisplayAlert("Сообщение", "Классификация изображений выполнена!", "OK");
        }
    }
}
