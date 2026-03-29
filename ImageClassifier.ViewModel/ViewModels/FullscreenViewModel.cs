using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageClassifier.Core.Interfaces;

namespace ImageClassifier.ViewModel.ViewModels
{
    public partial class FullscreenViewModel : ObservableObject
    {
        private readonly ITaskCommanderService _taskCommanderService;

        [ObservableProperty]
        private ImageSource? _selectedImage;

        [ObservableProperty]
        private ImageItemViewModel? _selectedImageItem;

        [ObservableProperty]
        private bool _isVisible;

        public FullscreenViewModel(
            ITaskCommanderService taskCommanderService) 
        {
            _taskCommanderService = taskCommanderService;
        }

        public async Task ShowImageAsync(ImageItemViewModel file)
        {
            if (!file.IsDeleted)
            {
                if (file.FullPath != null)
                {
                    var imageSource = ImageSource.FromFile(file.FullPath);
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        SelectedImage = imageSource;
                    });
                }
                IsVisible = true;
            } 
        }

        [RelayCommand]
        public void Hide()
        {
            IsVisible = false;
            SelectedImage = null;
        }

        [RelayCommand]
        private void SelectFile(ImageItemViewModel file)
        {
            SelectedImageItem = file;
            if (file.FilePreview != null)
                _taskCommanderService.AddTask(() => ShowImageAsync(file), true);
        }
    }
}
