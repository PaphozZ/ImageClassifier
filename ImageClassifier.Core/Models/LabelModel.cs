using System.Text.Json.Serialization;

namespace ImageClassifier.Core.Models
{
    public class LabelModel
    {
        public string Name { get; set; }
        public float Probability { get; set; }
        public DateTime LastModified { get; set; }

        [JsonConstructor]
        public LabelModel(string name, float probability, DateTime lastModified)
        {
            Name = name;
            Probability = probability;
            LastModified = lastModified;
        }
    }
}
