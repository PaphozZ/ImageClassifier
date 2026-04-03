using ImageClassifier.Core.Interfaces;
using SkiaSharp;

namespace ImageClassifier.Infrastructure.Services;

public class ImageTransformationService : IImageTransformationService
{
    public async Task<byte[]?> GenerateThumbnailAsync(string filePath, int minSize = 160)
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

    public async Task<List<string?>?> AugmentImages(List<string?>? imagePaths, int targetCount)
    {
        if (imagePaths == null || targetCount == 0) return null;

        return await Task.Run(() =>
        {
            try
            {
                var tempDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");

                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
                Directory.CreateDirectory(tempDir);

                var augmentedImages = new List<string?>();
                var random = new Random();
                var isFirstLoop = true;

                while (augmentedImages.Count < targetCount)
                {
                    foreach (var originalPath in imagePaths)
                    {
                        if (augmentedImages.Count >= targetCount) break;
                        if (string.IsNullOrEmpty(originalPath)) continue;

                        var newPath = AugmentSingleImage(originalPath, random, tempDir, isFirstLoop);
                        if (newPath != null)
                            augmentedImages.Add(newPath);
                    }
                    isFirstLoop = false;
                }
                return augmentedImages;
            }
            catch
            {
                return null;
            }
        });
    }

    private string? AugmentSingleImage(string originalPath, Random random, string tempDir, bool isFirstLoop)
    {
        using var originalBitmap = SKBitmap.Decode(originalPath);
        if (originalBitmap == null) return null;

        using var outputBitmap = new SKBitmap(originalBitmap.Width, originalBitmap.Height);
        using var canvas = new SKCanvas(outputBitmap);

        canvas.Clear(SKColors.Transparent);
        canvas.Save();

        float centerX = originalBitmap.Width / 2f;
        float centerY = originalBitmap.Height / 2f;
        canvas.Translate(centerX, centerY);
        canvas.RotateDegrees(random.Next(-15, 16));
        if (isFirstLoop || random.NextDouble() > 0.5)
        { 
            canvas.Scale(-1, 1);
        }
        canvas.Translate(-centerX, -centerY);

        canvas.DrawBitmap(originalBitmap, 0, 0);
        canvas.Restore();

        var cropped = CropEdges(outputBitmap, 0.05);
        SKBitmap imageToSave = cropped ?? outputBitmap;

        string newPath = Path.Combine(tempDir, $"{Guid.NewGuid()}.jpg");
        using (var image = SKImage.FromBitmap(imageToSave))
        using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 90))
        using (var stream = File.OpenWrite(newPath))
        {
            data.SaveTo(stream);
        }
        if (cropped != null) cropped.Dispose();

        return newPath;
    }

    private SKBitmap? CropEdges(SKBitmap bitmap, double percent)
    {
        int cropPxW = (int)(bitmap.Width * percent);
        int cropPxH = (int)(bitmap.Height * percent);
        int newW = bitmap.Width - 2 * cropPxW;
        int newH = bitmap.Height - 2 * cropPxH;
        if (newW <= 0 || newH <= 0) return null;

        var cropped = new SKBitmap(newW, newH);
        var rect = new SKRectI(cropPxW, cropPxH, bitmap.Width - cropPxW, bitmap.Height - cropPxH);
        bitmap.ExtractSubset(cropped, rect);
        return cropped;
    }
}