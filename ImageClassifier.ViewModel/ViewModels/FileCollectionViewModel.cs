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

        public async Task FillLabelsAsync(
            IEnumerable<ImageItemModel> labeledModels,
            IEnumerable<ImageItemViewModel> viewModels,
            IEnumerable<string> labels)
        {
            var modelDict = labeledModels.ToDictionary(m => m.FullPath);
            var allowedSet = new HashSet<string>(labels);

            foreach (var vm in viewModels)
            {
                modelDict.TryGetValue(vm.FullPath, out var model);

                foreach (var labelName in allowedSet)
                {
                    var existing = vm.Labels.Where(l => l.Name == labelName).ToList();
                    var labelData = model?.Labels.FirstOrDefault(l => l.Name == labelName);

                    if (existing != null)
                    {
                        foreach (var a in existing)
                            vm.Labels.Remove(a);
                    }
                    if (labelData != null)
                    {
                        vm.Labels.Add(new LabelViewModel(labelData.Name, labelData.Probability, labelData.LastModified));
                    }
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
