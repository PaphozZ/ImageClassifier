using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageClassifier.Core.Interfaces;
using ImageClassifier.ViewModel.Extensions;
using System.Collections.ObjectModel;
using System.Runtime.Versioning;
using System.Diagnostics;
using ImageClassifier.Core.Models;

namespace ImageClassifier.ViewModel.ViewModels
{
    public partial class PreviewViewModel : ObservableObject
    {
        [ObservableProperty]
        private ImageSource? _selectedImage;

        [ObservableProperty]
        private ObservableCollection<ImageItemViewModel> _loadedFiles = new();

        private readonly IFolderPicker _folderPicker;
        private readonly IFileStorageService _storageService;

        [ObservableProperty]
        private string? _currentFolderPath;

        [ObservableProperty]
        private bool _isFullScreen;

        public PreviewViewModel(IFolderPicker folderPicker, IFileStorageService storageService)
        {
            _folderPicker = folderPicker;
            _storageService = storageService;
            LoadSavedFilesAsync()
                .FireAndForget(ex => Debug.WriteLine($"Ошибка загрузки: {ex}"));
        }

        private async Task LoadSavedFilesAsync()
        {
            var files = await _storageService.LoadFilesAsync();

            LoadedFiles.Clear();
            foreach (var model in files)
            {
                var viewModel = new ImageItemViewModel(model);
                LoadedFiles.Add(viewModel);
                LoadImageAsync(viewModel)
                    .FireAndForget(ex => Debug.WriteLine($"Ошибка загрузки: {ex}"));
            }
        }

        private async Task LoadImageAsync(ImageItemViewModel item)
        {
            var FileFullName = (!string.IsNullOrEmpty(item.FileName) && !string.IsNullOrEmpty(item.FilePath))
                ? Path.Combine(item.FilePath, item.FileName) : null;

            if (FileFullName != null)
            {
                var imageSource = ImageSource.FromFile(FileFullName);
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    item.FilePreview = imageSource;
                });
            }
        }

        [RelayCommand]
        private async Task PickImageAsync()
        {
            try
            {
                var image = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Выберите изображение"
                });

                if (image != null)
                {
                    byte[] imageBytes;
                    using (var stream = await image.OpenReadAsync())
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await stream.CopyToAsync(memoryStream);
                            imageBytes = memoryStream.ToArray();
                        }
                    }

                    SelectedImage = ImageSource.FromStream(() => new MemoryStream(imageBytes));

                    var LoadedFile = new ImageItemViewModel
                    {
                        FileName = image.FileName,
                        FilePath = Path.GetDirectoryName(image.FullPath),
                        FilePreview = SelectedImage,
                        Extension = Path.GetExtension(image.FileName)
                    };

                    LoadedFiles.Add(LoadedFile);
                    await SaveFilesAsync();
                }
            }
            catch
            {
                Application.Current?.Windows[0].Page?.DisplayAlert("Ошибка", "Не удалось загрузить файл", "OK");
            }
        }

        private async Task SaveFilesAsync()
        {
            List<ImageItemModel> LoadedFilesModel = new();
            foreach (var file in LoadedFiles)
            {
                LoadedFilesModel.Add(new ImageItemModel
                {
                    FileName = file.FileName,
                    FilePath = file.FilePath,
                    LastModified = DateTime.Now
                });
            }
            await _storageService.SaveFilesAsync(LoadedFilesModel);
        }

        [RelayCommand]
        [SupportedOSPlatform("windows")]
        private async Task PickFolderAsync()
        {
            try
            {
                var status = await Permissions.RequestAsync<Permissions.StorageRead>();
                if (status != PermissionStatus.Granted)
                {
                    return;
                }
                var result = await _folderPicker.PickAsync(default);
                if (result.IsSuccessful)
                {
                    CurrentFolderPath = result.Folder.Path;
                    await LoadFilesFromFolderAsync(CurrentFolderPath);
                }
            }
            catch
            {
                Application.Current?.Windows[0].Page?.DisplayAlert("Ошибка", "Не удалось загрузить файлы", "OK");
            }
        }

        private async Task LoadFilesFromFolderAsync(string folderPath)
        {
            var fileItems = await Task.Run(() =>
            {
                var directoryInfo = new DirectoryInfo(folderPath);
                if (!directoryInfo.Exists)
                    return new List<ImageItemViewModel>();

                List<ImageItemViewModel> Images = new();
                foreach (var item in directoryInfo.GetFiles())
                {
                    if (item.Extension != null && IsImageFile(item.Extension))
                        Images.Add(new ImageItemViewModel
                        {
                            FileName = item.Name,
                            FilePath = item.DirectoryName,
                            FilePreview = ImageSource.FromFile(item.FullName),
                            Extension = item.Extension
                        });
                }
                return Images;
            });

            foreach (var item in fileItems)
                LoadedFiles.Add(item);

            await SaveFilesAsync();
        }

        private bool IsImageFile(string extension)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            return imageExtensions.Contains(extension.ToLower());
        }

        [RelayCommand]
        private void SelectFile(ImageItemViewModel file)
        {
            SelectedImage = file.FilePreview;
            IsFullScreen = true;
        }

        [RelayCommand]
        private void FullscreenTapped()
        {
            IsFullScreen = false;
            SelectedImage = null;
        }
    }
}
