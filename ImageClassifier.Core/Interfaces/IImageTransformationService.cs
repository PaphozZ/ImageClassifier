namespace ImageClassifier.Core.Interfaces;

public interface IImageTransformationService
{
    Task<byte[]?> GenerateThumbnailAsync(string filePath, int minSize = 160);
    Task<List<string?>?> AugmentImages(List<string?>? imagePaths, int targetCount);
}