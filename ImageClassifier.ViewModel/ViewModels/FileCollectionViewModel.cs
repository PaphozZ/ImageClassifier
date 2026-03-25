using CommunityToolkit.Mvvm.ComponentModel;
using ImageClassifier.Core.Enums;
using ImageClassifier.Core.Interfaces;
using ImageClassifier.Core.Models;
using System.Collections.ObjectModel;

namespace ImageClassifier.ViewModel.ViewModels
{
    public partial class FileCollectionViewModel : ObservableObject
    {
        private readonly IFileStorageService _storageService;
        private readonly IFileScanner _fileScanner;
        private readonly IImageResizeService _imageResizeService;
        private readonly ITaskCommanderService _taskCommanderService;

        [ObservableProperty]
        private ObservableCollection<ImageItemViewModel> _files = new();

        public FileCollectionViewModel(
            IFileStorageService storageService,
            IFileScanner fileScanner,
            IImageResizeService imageResizeService,
            ITaskCommanderService taskCommanderService)
        {
            _storageService = storageService;
            _fileScanner = fileScanner;
            _imageResizeService = imageResizeService;
            _taskCommanderService = taskCommanderService;
        }

        public async Task LoadSavedFilesAsync()
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
    }
}
