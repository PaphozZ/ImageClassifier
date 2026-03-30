using ImageClassifier.Core.Models;

namespace ImageClassifier.Core.Interfaces;

public interface IModelManagerService
{
    Task SaveModelAsync(ModelItemModel model, byte[] modelData);
    Task<ModelItemModel?> GetModelByLabelAsync(string label);
    Task<IEnumerable<ModelItemModel>> GetAllModelsAsync();
    Task<byte[]?> LoadModelDataAsync(string label);
    Task DeleteModelAsync(string label);
}