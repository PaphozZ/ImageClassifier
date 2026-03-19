using CommunityToolkit.Mvvm.ComponentModel;

namespace ImageClassifier.ViewModel.ViewModels
{
    public partial class FullscreenViewModel : ObservableObject
    {
        [ObservableProperty]
        private ImageSource? _selectedImage;

        [ObservableProperty]
        private bool _isVisible;

        public void ShowImage(ImageSource imageSource)
        {
            SelectedImage = imageSource;
            IsVisible = true;
        }

        public void Hide()
        {
            IsVisible = false;
            SelectedImage = null;
        }
    }
}
