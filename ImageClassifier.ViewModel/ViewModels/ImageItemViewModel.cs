namespace ImageClassifier.ViewModel.ViewModels
{
    public class ImageItemViewModel
    {
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public ImageSource? FilePreview { get; set; }
        public string? Extension { get; set; }
    }
}
