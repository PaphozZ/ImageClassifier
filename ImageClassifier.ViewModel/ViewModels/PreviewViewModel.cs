using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Runtime.Versioning;

namespace ImageClassifier.ViewModel.ViewModels
{
    public partial class PreviewViewModel : ObservableObject
    {
        [ObservableProperty]
        private ImageSource? _selectedImage;

        [ObservableProperty]
        private ObservableCollection<ImageItemViewModel> _loadedFiles = new();

        private readonly IFolderPicker _folderPicker;

        [ObservableProperty]
        private string? _currentFolderPath;

        public PreviewViewModel(IFolderPicker folderPicker)
        {
            _folderPicker = folderPicker;
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
                    LoadedFiles.Add(new ImageItemViewModel { FilePreview = SelectedImage });
                }
            }
            catch 
            {
                Application.Current?.Windows[0].Page?.DisplayAlert("Ошибка", "Не удалось загрузить файл", "OK");
            }
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
                            FilePath = item.FullName,
                            FilePreview = ImageSource.FromFile(item.FullName),
                            Extension = item.Extension
                        });
                }
                return Images;
            });

            foreach (var item in fileItems)
                LoadedFiles.Add(item);
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
        }
    }
}
