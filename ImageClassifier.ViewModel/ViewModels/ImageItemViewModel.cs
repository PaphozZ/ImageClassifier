using CommunityToolkit.Mvvm.ComponentModel;
using ImageClassifier.Core.Interfaces;
using ImageClassifier.Core.Models;
using ImageClassifier.ViewModel.Extensions;
using System.Diagnostics;

namespace ImageClassifier.ViewModel.ViewModels;

public partial class ImageItemViewModel : ObservableObject
{
    private readonly IThumbnailService _thumbnailService;

    public string FileName { get; }
    public string FilePath { get; }
    public DateTime LastModified { get; }
    public long Size { get; }

    [ObservableProperty]
    private ImageSource? _filePreview;

    public ImageItemViewModel(ImageItemModel model, IThumbnailService thumbnailService)
    {
        _thumbnailService = thumbnailService;

        FileName = model.FileName;
        FilePath = model.FilePath;
        LastModified = model.LastModified;
        Size = model.Size;

        LoadThumbnailAsync()
            .FireAndForget(ex => Debug.WriteLine($"Ошибка загрузки: {ex}"));
    }

    public async Task LoadThumbnailAsync()
    {
        var fullPath = Path.Combine(FilePath, FileName);
        if (File.Exists(fullPath))
        {
            var bytes = await _thumbnailService.GenerateThumbnailAsync(fullPath);
            if (bytes != null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    FilePreview = ImageSource.FromStream(() => new MemoryStream(bytes));
                });
            }
        }
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