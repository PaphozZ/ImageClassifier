using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ImageClassifier.ViewModel.ViewModels
{
    public partial class FullscreenViewModel : ObservableObject
    {
        [ObservableProperty]
        private ImageSource? _selectedImage;

        [ObservableProperty]
        private bool _isVisible;

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
    }
}
