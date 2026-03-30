using CommunityToolkit.Mvvm.ComponentModel;
using ImageClassifier.Core.Enums;
using ImageClassifier.Core.Interfaces;
using ImageClassifier.Core.Models;
using System.Collections.ObjectModel;

namespace ImageClassifier.ViewModel.ViewModels
{
    public partial class FileCollectionViewModel : ObservableObject
    {
        private readonly IJsonStorageService<ImageItemModel> _storageService;
        private readonly IFileScanner _fileScanner;
        private readonly IImageResizeService _imageResizeService;
        private readonly ITaskCommanderService _taskCommanderService;

        [ObservableProperty]
        private ObservableCollection<ImageItemViewModel> _files = new();

        [ObservableProperty]
        private ObservableCollection<ImageItemViewModel> _positiveItems = new();
        [ObservableProperty]
        private ObservableCollection<ImageItemViewModel> _negativeItems = new();

        public FileCollectionViewModel(
            IJsonStorageService<ImageItemModel> storageService,
            IFileScanner fileScanner,
            IImageResizeService imageResizeService,
            ITaskCommanderService taskCommanderService)
        {
            _storageService = storageService;
            _fileScanner = fileScanner;
            _imageResizeService = imageResizeService;
            _taskCommanderService = taskCommanderService;

            _taskCommanderService.AddTask(LoadSavedFilesAsync);
        }

        private async Task LoadSavedFilesAsync()
        {
            var models = await _storageService.LoadFilesAsync();
            Files.Clear();
            foreach (var model in models)
                Files.Add(new ImageItemViewModel(model, _imageResizeService, _taskCommanderService));
        }

        public async Task AddFilesFromFolderAsync(string folderPath)
        {
            var models = await _fileScanner.ScanFolderAsync(folderPath);
            foreach (var model in models)
                Files.Add(new ImageItemViewModel(model, _imageResizeService, _taskCommanderService));
            await SaveAsync();
        }

        public async Task AddFileAsync(ImageItemModel model)
        {
            Files.Add(new ImageItemViewModel(model, _imageResizeService, _taskCommanderService));
            await SaveAsync();
        }

        public async Task RemoveFileAsync(ImageItemViewModel item)
        {
            item.FilePreview = ImageSource.FromFile("waste_basket.png");
            item.IsDeleted = true;
            await SaveAsync();
        }

        public Task ResetDatasetClasses()
        {
            foreach (var file in Files)
                file.DatasetClass = DatasetClass.None;
            return Task.CompletedTask;
        }

        private async Task SaveAsync()
        {
            var files = Files.Where(f => !f.IsDeleted);
            var models = files.Select(f => f.ToModel()).ToList();
            await _storageService.SaveFilesAsync(models);
        }

        public async Task FillLabelsAsync(IEnumerable<ImageItemModel> items)
        {
            foreach (var item in items)
            {
                var file = Files.FirstOrDefault(f => f.FullPath == item.FullPath);
                var labelData = item.Labels.LastOrDefault();
                if (file != null && labelData != null)
                {
                    var label = new LabelViewModel(
                        name: labelData.Name,
                        probability: labelData.Probability,
                        modelId: labelData.ModelId,
                        lastModified: labelData.LastModified
                    );
                    file.Labels.Add(label);
                }
            }
            await SaveAsync();
        }

        public void ClearZones()
        {
            PositiveItems.Clear();
            NegativeItems.Clear();
        }
    }
}
