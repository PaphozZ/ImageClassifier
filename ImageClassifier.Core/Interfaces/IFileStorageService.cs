using ImageClassifier.Core.Models;

namespace ImageClassifier.Core.Interfaces
{
    public interface IFileStorageService
    {
        Task<List<ImageItemModel>> LoadFilesAsync();
        Task SaveFilesAsync(IEnumerable<ImageItemModel> files);
    }
}