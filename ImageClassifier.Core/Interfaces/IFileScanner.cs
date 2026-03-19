using ImageClassifier.Core.Models;

namespace ImageClassifier.Core.Interfaces;

public interface IFileScanner
{
    Task<IEnumerable<ImageItemModel>> ScanFolderAsync(string folderPath);
}