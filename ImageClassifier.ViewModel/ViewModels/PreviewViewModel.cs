using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ImageClassifier.ViewModel.ViewModels
{
    public partial class PreviewViewModel : ObservableObject
    {
        [ObservableProperty]
        private ImageSource? _selectedImage;

        [RelayCommand]
        private async Task PickImageAsync()
        {
            try
            {
                var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Выберите изображение"
                });

                if (photo != null)
                {
                    byte[] imageBytes;
                    using (var stream = await photo.OpenReadAsync())
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await stream.CopyToAsync(memoryStream);
                            imageBytes = memoryStream.ToArray();
                        }
                    }

                    SelectedImage = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                }
            }
            catch { }
        }
    }
}
