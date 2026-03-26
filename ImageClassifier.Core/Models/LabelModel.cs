using System.Text.Json.Serialization;

namespace ImageClassifier.Core.Models
{
    public class LabelModel
    {
        public string Name { get; set; }
        public float Probability { get; set; }
        public Guid ModelId { get; set; }
        public DateTime LastModified { get; set; }

        [JsonConstructor]
        public LabelModel(string name, float probability, Guid modelId, DateTime lastModified)
        {
            Name = name;
            Probability = probability;
            ModelId = modelId;
            LastModified = lastModified;
        }
    }
}
