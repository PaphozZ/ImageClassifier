using System.Text.Json;
using ImageClassifier.Core.Interfaces;

namespace ImageClassifier.Infrastructure.Services;

public class JsonStorageService<T> : IJsonStorageService<T> where T : class
{
    private readonly string _filePath;
    public JsonStorageService(string fileName)
    {
        string appData = AppContext.BaseDirectory;
        _filePath = Path.Combine(appData, fileName);
    }

    public async Task<List<T>> LoadFilesAsync()
    {
        if (!File.Exists(_filePath))
            return new List<T>();
        try
        {
            string json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
        }
        catch
        {
            return new List<T>();
        }
    }

    public async Task SaveFilesAsync(IEnumerable<T> files)
    {
        string json = JsonSerializer.Serialize(files);
        await File.WriteAllTextAsync(_filePath, json);
    }
}