using ImageClassifier.Core.Interfaces;
using ImageClassifier.Core.Models;

namespace ImageClassifier.Infrastructure.Services;

public class ModelManagerService : IModelManagerService
{
    private readonly IJsonStorageService<ModelItemModel> _storage;

    private readonly string _modelsDirectory;

    public ModelManagerService(IJsonStorageService<ModelItemModel> storage)
    {
        _storage = storage;
        _modelsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models");
        if (!Directory.Exists(_modelsDirectory))
            Directory.CreateDirectory(_modelsDirectory);
    }

    public async Task SaveModelAsync(ModelItemModel model, byte[] modelData)
    {
        var modelFilePath = Path.Combine(_modelsDirectory, model.FileName);
        await File.WriteAllBytesAsync(modelFilePath, modelData);

        var models = await _storage.LoadFilesAsync();
        var existing = models.FirstOrDefault(m => m.LabelName == model.LabelName);
        if (existing != null)
            models.Remove(existing);
        models.Add(model);
        await _storage.SaveFilesAsync(models);
    }

    public async Task<ModelItemModel?> GetModelByLabelAsync(string label)
    {
        var models = await _storage.LoadFilesAsync();
        return models.FirstOrDefault(m => m.LabelName == label);
    }

    public async Task<IEnumerable<ModelItemModel>> GetAllModelsAsync()
    {
        return await _storage.LoadFilesAsync();
    }

    public async Task<byte[]?> LoadModelDataAsync(string label)
    {
        var model = await GetModelByLabelAsync(label);
        if (model == null) return null;

        var modelPath = Path.Combine(_modelsDirectory, model.FileName);
        if (!File.Exists(modelPath)) return null;

        return await File.ReadAllBytesAsync(modelPath);
    }

    public async Task DeleteModelAsync(string label)
    {
        var model = await GetModelByLabelAsync(label);
        if (model == null) return;

        var modelPath = Path.Combine(_modelsDirectory, model.FileName);
        if (File.Exists(modelPath))
            File.Delete(modelPath);

        var models = await _storage.LoadFilesAsync();
        models.RemoveAll(m => m.LabelName == label);
        await _storage.SaveFilesAsync(models);
    }
}