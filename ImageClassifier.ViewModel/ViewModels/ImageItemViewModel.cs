using ImageClassifier.Core.Models;

namespace ImageClassifier.ViewModel.ViewModels;

public class ImageItemViewModel
{
    public string FileName { get; }
    public string FilePath { get; }
    public DateTime LastModified { get; }
    public long Size { get; }

    public ImageSource? FilePreview { get; set; }

    public ImageItemViewModel(ImageItemModel model)
    {
        FileName = model.FileName;
        FilePath = model.FilePath;
        LastModified = model.LastModified;
        Size = model.Size;

        var fullName = Path.Combine(FilePath, FileName);
        FilePreview = ImageSource.FromFile(fullName);
    }

    public ImageItemModel ToModel()
    {
        return new ImageItemModel
        {
            FileName = FileName,
            FilePath = FilePath,
            LastModified = LastModified,
            Size = Size
        };
    }
}