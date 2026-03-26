using ImageClassifier.Core.Models;
using Microsoft.Maui.Storage;

namespace ImageClassifier.ViewModel.ViewModels
{
    public class LabelViewModel
    {
        public string Name { get; }
        public float Probability { get; }
        public Guid ModelId { get; }
        public DateTime LastModified { get; }

        public LabelViewModel(string name, float probability, Guid modelId, DateTime lastModified) 
        {
            Name = name;
            Probability = probability;
            ModelId = modelId;
            LastModified = lastModified;
        }

        public LabelModel ToModel()
        {
            return new LabelModel(
                name: Name,
                probability: Probability,
                modelId: ModelId,
                lastModified: LastModified);
        }
    }
}
