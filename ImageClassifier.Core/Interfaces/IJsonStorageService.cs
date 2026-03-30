namespace ImageClassifier.Core.Interfaces
{
    public interface IJsonStorageService<T> where T : class
    {
        Task<List<T>> LoadFilesAsync();
        Task SaveFilesAsync(IEnumerable<T> files);
    }
}