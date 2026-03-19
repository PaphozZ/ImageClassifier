using ImageClassifier.Core.Models;

namespace ImageClassifier.ViewModel.ViewModels
{
    public class ImageItemViewModel
    {
        public ImageItemViewModel() { }
        public ImageItemViewModel(ImageItemModel model)
        {
            FileName = model.FileName;
            FilePath = model.FilePath;
        }

        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public ImageSource? FilePreview { get; set; }
        public string? Extension { get; set; }
    }
}
