using CommunityToolkit.Mvvm.ComponentModel;

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
            {
                var FileFullName = (!string.IsNullOrEmpty(file.FileName) && !string.IsNullOrEmpty(file.FilePath))
                ? Path.Combine(file.FilePath, file.FileName) : null;

                if (FileFullName != null)
                {
                    var imageSource = ImageSource.FromFile(FileFullName);
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        SelectedImage = imageSource;
                    });
                }
            }
            IsVisible = true;
        }

        public void Hide()
        {
            IsVisible = false;
            SelectedImage = null;
        }
    }
}
