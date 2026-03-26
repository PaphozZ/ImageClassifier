using CommunityToolkit.Mvvm.ComponentModel;
using ImageClassifier.Core.Enums;
using ImageClassifier.Core.Interfaces;
using ImageClassifier.Core.Models;
using System.Collections.ObjectModel;

namespace ImageClassifier.ViewModel.ViewModels;

public partial class ImageItemViewModel : ObservableObject
{
    private readonly IImageResizeService _imageResizeService;
    private readonly ITaskCommanderService _taskCommanderService;

    public string FileName { get; }
    public string FilePath { get; }
    public string FullPath { get; }
    public long Size { get; }
    public DateTime LastModified { get; }
    public ObservableCollection<LabelViewModel> Labels { get; } = new();

    [ObservableProperty]
    private bool _isDeleted;

    [ObservableProperty]
    private DatasetClass _datasetClass = DatasetClass.None;

    [ObservableProperty]
    private ImageSource? _filePreview;

    public ImageItemViewModel(ImageItemModel model, IImageResizeService imageResizeService, ITaskCommanderService taskCommanderService)
    {
        _imageResizeService = imageResizeService;
        _taskCommanderService = taskCommanderService;

        FileName = model.FileName;
        FilePath = model.FilePath;
        FullPath = model.FullPath;
        Size = model.Size;
        LastModified = model.LastModified;

        foreach (var label in model.Labels)
            Labels.Add(new(label.Name, label.Probability, label.ModelId, label.LastModified));

        _taskCommanderService.AddTask(LoadThumbnailAsync);
    }

    public async Task LoadThumbnailAsync()
    {
        if (!IsDeleted && File.Exists(FullPath))
        {
            var bytes = await _imageResizeService.GenerateThumbnailAsync(FullPath);
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
        return new ImageItemModel(
            fileName: FileName,
            filePath: FilePath,
            fullPath: FullPath,
            size: Size,
            lastModified: LastModified,
            labels: Labels.Select(l => l.ToModel()).ToList());
    }
}