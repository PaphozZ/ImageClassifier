using ImageClassifier.Core.Interfaces;
using ImageClassifier.Core.Models;

namespace ImageClassifier.Infrastructure.Services;

public class MediaPickerService : IMediaPickerService
{
    public async Task<ImageItemModel?> PickImageAsync()
    {
        try
        {
            var image = await MediaPicker.Default.PickPhotoAsync();
            if (image == null) return null;

            return new ImageItemModel
            {
                FileName = image.FileName,
                FilePath = Path.GetDirectoryName(image.FullPath) ?? string.Empty,
                LastModified = DateTime.Now
            };
        }
        catch
        {
            return null;
        }
    }
}