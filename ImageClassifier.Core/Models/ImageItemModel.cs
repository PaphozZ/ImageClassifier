namespace ImageClassifier.Core.Models
{
    public class ImageItemModel
    {
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public DateTime LastModified { get; set; }
        public long Size { get; set; }
        public bool IsImage { get; set; }
    }
}
