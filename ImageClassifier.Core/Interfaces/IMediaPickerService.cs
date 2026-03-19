using ImageClassifier.Core.Models;

namespace ImageClassifier.Core.Interfaces;

public interface IMediaPickerService
{
    Task<ImageItemModel?> PickImageAsync();
}