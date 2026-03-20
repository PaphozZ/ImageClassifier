namespace ImageClassifier.Core.Interfaces;

public interface IThumbnailService
{
    Task<byte[]?> GenerateThumbnailAsync(string filePath, int maxSize = 224);
}