using CommunityToolkit.Mvvm.ComponentModel;
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

        [ObservableProperty]
        private ObservableCollection<ImageItemViewModel> _files = new();

        public FileCollectionViewModel(IFileStorageService storageService, IFileScanner fileScanner, IThumbnailService thumbnailService)
        {
            _storageService = storageService;
            _fileScanner = fileScanner;
            _thumbnailService = thumbnailService;
        }

        public async Task LoadSavedFilesAsync()
        {
            var models = await _storageService.LoadFilesAsync();
            Files.Clear();
            foreach (var model in models)
                Files.Add(new ImageItemViewModel(model, _thumbnailService));
        }

        public async Task AddFilesFromFolderAsync(string folderPath)
        {
            var models = await _fileScanner.ScanFolderAsync(folderPath);
            foreach (var model in models)
                Files.Add(new ImageItemViewModel(model, _thumbnailService));
            await SaveAsync();
        }

        public async Task AddFileAsync(ImageItemModel model)
        {
            Files.Add(new ImageItemViewModel(model, _thumbnailService));
            await SaveAsync();
        }

        private async Task SaveAsync()
        {
            var models = Files.Select(f => f.ToModel()).ToList();
            await _storageService.SaveFilesAsync(models);
        }
    }
}
