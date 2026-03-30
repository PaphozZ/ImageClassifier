using ImageClassifier.Core.Interfaces;
using ImageClassifier.Core.Models;

namespace ImageClassifier.Infrastructure.Services;

public class MediaPickerService : IMediaPickerService
{
    public async Task<ImageItemModel?> PickImageAsync()
    {
        try
        {
            var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "image/*" } },
                    { DevicePlatform.iOS, new[] { "public.image" } },
                    { DevicePlatform.WinUI, new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" } }
                });
            var options = new PickOptions
            {
                PickerTitle = "Выберите изображение",
                FileTypes = customFileType,
            };
            var image = await FilePicker.Default.PickAsync(options);
            if (image == null) return null;
            var fileInfo = new FileInfo(image.FullPath);
            return new ImageItemModel(
                fileName: image.FileName,
                filePath: Path.GetDirectoryName(image.FullPath)!,
                fullPath: image.FullPath,
                size: fileInfo.Length,
                lastModified: DateTime.Now,
                labels: new());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка при выборе файла: {ex.Message}");
            return null;
        }
    }
}