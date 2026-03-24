namespace ImageClassifier.Core.Models
{
    public class ImageItemModel
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
        public long Size { get; set; }
        public bool IsImage { get; set; }
    }
}
