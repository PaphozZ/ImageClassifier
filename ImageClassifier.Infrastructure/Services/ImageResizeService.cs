using ImageClassifier.Core.Interfaces;
using SkiaSharp;

namespace ImageClassifier.Infrastructure.Services;

public class ImageResizeService : IImageResizeService
{
    public async Task<byte[]?> GenerateThumbnailAsync(string filePath, int minSize = 80)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var original = SKBitmap.Decode(filePath);
                if (original == null) return null;

                float scale = (float)minSize / Math.Min(original.Width, original.Height);
                int newWidth = (int)(original.Width * scale);
                int newHeight = (int)(original.Height * scale);

                using var scaled = original.Resize(new SKImageInfo(newWidth, newHeight), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));

                using var image = SKImage.FromBitmap(scaled);
                using var data = image.Encode(SKEncodedImageFormat.Png, 90);
                return data.ToArray();
            }
            catch
            {
                return null;
            }
        });
    }

    public async Task<byte[]?> ResizeTo224(string imagePath)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var original = SKBitmap.Decode(imagePath);
                if (original == null) return null;

                using var resized = new SKBitmap(224, 224);
                original.ScalePixels(resized, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));

                using var image = SKImage.FromBitmap(resized);
                using var data = image.Encode(SKEncodedImageFormat.Png, 90);
                return data.ToArray();
            }
            catch
            {
                return null;
            }
        });
    }
}