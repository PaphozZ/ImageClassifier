using ImageClassifier.Core.Models;

namespace ImageClassifier.Core.Interfaces
{
    public interface IPredictionService
    {
        Task<IEnumerable<ImageItemModel>> ApplyPredictionsAsync(IEnumerable<ImageItemModel> items, string label);
    }
}
