namespace ImageClassifier.Core.Interfaces;

public interface IImageResizeService
{
    Task<byte[]?> GenerateThumbnailAsync(string filePath, int maxSize = 224);
    Task<byte[]?> ResizeTo224(string imagePath);
}