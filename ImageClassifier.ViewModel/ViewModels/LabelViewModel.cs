using CommunityToolkit.Mvvm.ComponentModel;
using ImageClassifier.Core.Models;

namespace ImageClassifier.ViewModel.ViewModels
{
    public partial class LabelViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name;
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
