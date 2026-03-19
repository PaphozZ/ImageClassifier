using System.Text.Json;
using ImageClassifier.Core.Models;
using ImageClassifier.Core.Interfaces;

namespace ImageClassifier.Infrastructure.Services;

public class JsonFileStorageService : IFileStorageService
{
    private readonly string _filePath;

    public JsonFileStorageService()
    {
        string appData = AppContext.BaseDirectory;
        _filePath = Path.Combine(appData, "loaded_files.json");
    }

    public async Task<List<ImageItemModel>> LoadFilesAsync()
    {
        if (!File.Exists(_filePath))
            return new List<ImageItemModel>();

        try
        {
            string json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<ImageItemModel>>(json) ?? new List<ImageItemModel>();
        }
        catch
        {
            return new List<ImageItemModel>();
        }
    }

    public async Task SaveFilesAsync(IEnumerable<ImageItemModel> files)
    {
        string json = JsonSerializer.Serialize(files);
        await File.WriteAllTextAsync(_filePath, json);
    }
}