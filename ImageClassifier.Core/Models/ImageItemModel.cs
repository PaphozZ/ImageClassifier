using System.Text.Json.Serialization;

namespace ImageClassifier.Core.Models
{
    public class ImageItemModel
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FullPath { get; set; }
        public long Size { get; set; }
        public string? Hash { get; set; }
        public DateTime LastModified { get; set; }
        public List<LabelModel> Labels { get; set; } = new();

        [JsonConstructor]
        public ImageItemModel(string fileName, string filePath, string fullPath, long size, DateTime lastModified, List<LabelModel> labels)
        {
            FileName = fileName;
            FilePath = filePath;
            FullPath = fullPath;
            Size = size;
            LastModified = lastModified;
            Labels = labels;
        }
    }
}
