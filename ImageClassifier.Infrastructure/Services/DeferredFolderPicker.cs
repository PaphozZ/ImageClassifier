using CommunityToolkit.Maui.Storage;
using System.Runtime.Versioning;

namespace ImageClassifier.Infrastructure.Services
{
    [SupportedOSPlatform("windows")]
    public class DeferredFolderPicker : IFolderPicker
    {
        private IFolderPicker? _inner;
        private IFolderPicker Inner => _inner ??= FolderPicker.Default;

        public Task<FolderPickerResult> PickAsync(CancellationToken cancellationToken = default)
        {
            return Inner.PickAsync(cancellationToken);
        }

        public Task<FolderPickerResult> PickAsync(string initialPath, CancellationToken cancellationToken = default)
        {
            return Inner.PickAsync(initialPath, cancellationToken);
        }
    }
}
