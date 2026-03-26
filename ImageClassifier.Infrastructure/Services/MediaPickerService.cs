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

            return new ImageItemModel(
                fileName: image.FileName,
                filePath: Path.GetDirectoryName(image.FullPath)!,
                fullPath: image.FullPath,
                size: new FileInfo(image.FullPath).Length,
                lastModified: DateTime.Now,
                labels: new());
        }
        catch
        {
            return null;
        }
    }
}