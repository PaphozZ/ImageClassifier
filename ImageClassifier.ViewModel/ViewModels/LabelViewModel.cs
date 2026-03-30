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

        public LabelViewModel(string name, float probability, DateTime lastModified) 
        {
            Name = name;
            Probability = probability;
            LastModified = lastModified;
        }

        public LabelModel ToModel()
        {
            return new LabelModel(
                name: Name,
                probability: Probability,
                lastModified: LastModified);
        }
    }
}
