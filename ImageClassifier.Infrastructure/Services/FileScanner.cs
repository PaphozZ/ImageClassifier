using ImageClassifier.Core.Interfaces;
using ImageClassifier.Core.Models;

namespace ImageClassifier.Infrastructure.Services;

public class FileScanner : IFileScanner
{
    private string[] _imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };

    public Task<IEnumerable<ImageItemModel>> ScanFolderAsync(string folderPath)
    {
        return Task.Run(() =>
        {
            var dir = new DirectoryInfo(folderPath);
            if (!dir.Exists) return Enumerable.Empty<ImageItemModel>();

            return dir.GetFiles()
                .Where(f => IsImageFile(f.Extension))
                .Select(f => new ImageItemModel
                {
                    FileName = f.Name,
                    FilePath = f.DirectoryName ?? string.Empty,
                    FullPath = f.FullName,
                    LastModified = f.LastWriteTime,
                    Size = f.Length
                });
        });
    }

    private bool IsImageFile(string extension) =>
        _imageExtensions.Contains(extension?.ToLower() ?? string.Empty);
}