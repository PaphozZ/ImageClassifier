using CommunityToolkit.Maui.Storage;

namespace ImageClassifier.Infrastructure
{
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
