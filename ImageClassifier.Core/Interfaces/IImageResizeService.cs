namespace ImageClassifier.Core.Interfaces;

public interface IImageResizeService
{
    Task<byte[]?> GenerateThumbnailAsync(string filePath, int minSize = 160);
}