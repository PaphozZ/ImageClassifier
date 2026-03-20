using ImageClassifier.Core.Interfaces;
using SkiaSharp;

namespace ImageClassifier.Infrastructure.Services;

public class ThumbnailService : IThumbnailService
{
    public async Task<byte[]?> GenerateThumbnailAsync(string filePath, int maxSize = 224)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var original = SKBitmap.Decode(filePath);
                if (original == null) return null;

                float scale = (float)maxSize / Math.Max(original.Width, original.Height);
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
}