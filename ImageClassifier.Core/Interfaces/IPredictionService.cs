using ImageClassifier.Core.Models;

namespace ImageClassifier.Core.Interfaces
{
    public interface IPredictionService
    {
        Task ApplyPredictionsAsync(IEnumerable<ImageItemModel> items);
    }
}
