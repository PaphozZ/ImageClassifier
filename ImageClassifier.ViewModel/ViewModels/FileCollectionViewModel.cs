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
        private readonly IThumbnailService _thumbnailService;
        private readonly ITaskCommanderService _taskCommanderService;

        [ObservableProperty]
        private ObservableCollection<ImageItemViewModel> _files = new();

        public FileCollectionViewModel(
            IFileStorageService storageService,
            IFileScanner fileScanner,
            IThumbnailService thumbnailService,
            ITaskCommanderService taskCommanderService)
        {
            _storageService = storageService;
            _fileScanner = fileScanner;
            _thumbnailService = thumbnailService;
            _taskCommanderService = taskCommanderService;
        }

        public async Task LoadSavedFilesAsync()
        {
            var models = await _storageService.LoadFilesAsync();
            Files.Clear();
            foreach (var model in models)
                Files.Add(new ImageItemViewModel(model, _thumbnailService, _taskCommanderService));
        }

        public async Task AddFilesFromFolderAsync(string folderPath)
        {
            var models = await _fileScanner.ScanFolderAsync(folderPath);
            foreach (var model in models)
                Files.Add(new ImageItemViewModel(model, _thumbnailService, _taskCommanderService));
            await SaveAsync();
        }

        public async Task AddFileAsync(ImageItemModel model)
        {
            Files.Add(new ImageItemViewModel(model, _thumbnailService, _taskCommanderService));
            await SaveAsync();
        }

        public void RemoveFile(ImageItemViewModel item)
        {
            item.FilePreview = ImageSource.FromFile("waste_basket.png");
            item.IsDeleted = true;
            _taskCommanderService.AddTask(SaveAsync, true);
        }

        public void ResetDatasetClasses()
        {
            foreach (var file in Files)
                file.DatasetClass = DatasetClass.None;
        }

        private async Task SaveAsync()
        {
            var files = Files.Where(f => !f.IsDeleted);
            var models = files.Select(f => f.ToModel()).ToList();
            await _storageService.SaveFilesAsync(models);
        }
    }
}
