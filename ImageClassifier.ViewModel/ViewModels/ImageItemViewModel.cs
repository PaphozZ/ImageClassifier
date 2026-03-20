using CommunityToolkit.Mvvm.ComponentModel;
using ImageClassifier.Core.Interfaces;
using ImageClassifier.Core.Models;
using ImageClassifier.ViewModel.Extensions;
using System.Diagnostics;

namespace ImageClassifier.ViewModel.ViewModels;

public partial class ImageItemViewModel : ObservableObject
{
    private readonly IThumbnailService _thumbnailService;
    private readonly ITaskCommanderService _taskCommanderService;

    public string FileName { get; }
    public string FilePath { get; }
    public DateTime LastModified { get; }
    public long Size { get; }

    [ObservableProperty]
    private ImageSource? _filePreview;

    public ImageItemViewModel(ImageItemModel model, IThumbnailService thumbnailService, ITaskCommanderService taskCommanderService)
    {
        _thumbnailService = thumbnailService;
        _taskCommanderService = taskCommanderService;

        FileName = model.FileName;
        FilePath = model.FilePath;
        LastModified = model.LastModified;
        Size = model.Size;

        _taskCommanderService.AddTask(() => LoadThumbnailAsync());
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