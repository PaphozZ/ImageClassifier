using ImageClassifier.Core.Models;

namespace ImageClassifier.Core.Interfaces;

public interface IModelTrainingService
{
    Task TrainAsync(IEnumerable<ImageItemModel> positiveItems, IEnumerable<ImageItemModel> negativeItems);
}