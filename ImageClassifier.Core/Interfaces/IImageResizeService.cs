namespace ImageClassifier.Core.Interfaces;

public interface IImageResizeService
{
    Task<byte[]?> GenerateThumbnailAsync(string filePath, int minSize = 160);
    Task<byte[]?> ResizeTo224(string imagePath);
}