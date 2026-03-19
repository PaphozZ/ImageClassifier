using CommunityToolkit.Mvvm.ComponentModel;
using ImageClassifier.Core.Interfaces;
using ImageClassifier.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageClassifier.ViewModel.ViewModels
{
    public partial class FileCollectionViewModel : ObservableObject
    {
        private readonly IFileStorageService _storageService;
        private readonly IFileScanner _fileScanner;

        [ObservableProperty]
        private ObservableCollection<ImageItemViewModel> _files = new();

        public FileCollectionViewModel(IFileStorageService storageService, IFileScanner fileScanner)
        {
            _storageService = storageService;
            _fileScanner = fileScanner;
        }

        public async Task LoadSavedFilesAsync()
        {
            var models = await _storageService.LoadFilesAsync();
            Files.Clear();
            foreach (var model in models)
                Files.Add(new ImageItemViewModel(model));
        }

        public async Task AddFilesFromFolderAsync(string folderPath)
        {
            var models = await _fileScanner.ScanFolderAsync(folderPath);
            foreach (var model in models)
                Files.Add(new ImageItemViewModel(model));
            await SaveAsync();
        }

        public async Task AddFileAsync(ImageItemModel model)
        {
            Files.Add(new ImageItemViewModel(model));
            await SaveAsync();
        }

        private async Task SaveAsync()
        {
            var models = Files.Select(f => f.ToModel()).ToList();
            await _storageService.SaveFilesAsync(models);
        }
    }
}
