using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageClassifier.Core.Enums;
using ImageClassifier.ViewModel.Enums;
using ImageClassifier.ViewModel.ViewModels;

public partial class DragDropManagerViewModel : ObservableObject
{
    private readonly FileCollectionViewModel _fileCollection;
    private readonly ModeManagerViewModel _modeManager;

    private object? _draggedItem;

    public DragDropManagerViewModel(
        FileCollectionViewModel fileCollection,
        ModeManagerViewModel modeManager)
    {
        _fileCollection = fileCollection;
        _modeManager = modeManager;
    }

    [RelayCommand]
    private void DragStarting(object item) => _draggedItem = item;

    [RelayCommand]
    private void DropToPositiveItems()
    {
        if (_draggedItem != null)
        {
            if (_draggedItem is ImageItemViewModel draggedImageItem)
            {
                if (!draggedImageItem.IsDeleted && !_fileCollection.PositiveItems.Contains(draggedImageItem))
                {
                    _fileCollection.PositiveItems.Add(draggedImageItem);
                    if (_fileCollection.NegativeItems.Contains(draggedImageItem))
                        _fileCollection.NegativeItems.Remove(draggedImageItem);
                    draggedImageItem.DatasetClass = DatasetClass.Positive;
                    _draggedItem = null;

                    if (_modeManager.CurrentMode == AppMode.Predict)
                    {
                        _modeManager.IsNegativeVisible = false;
                    }
                }
            }
        }
    }

    [RelayCommand]
    private async Task DropToNegativeItems()
    {
        if (_draggedItem != null)
        {
            if (_draggedItem is ImageItemViewModel draggedImageItem)
            {
                if (!draggedImageItem.IsDeleted && !_fileCollection.NegativeItems.Contains(draggedImageItem))
                {
                    if (_modeManager.CurrentMode == AppMode.Preview)
                    {
                        await _fileCollection.RemoveFileAsync(draggedImageItem);
                    }
                    else
                    {
                        _fileCollection.NegativeItems.Add(draggedImageItem);
                        if (_fileCollection.PositiveItems.Contains(draggedImageItem))
                            _fileCollection.PositiveItems.Remove(draggedImageItem);
                        draggedImageItem.DatasetClass = DatasetClass.Negative;

                        if (_modeManager.CurrentMode == AppMode.Predict)
                        {
                            _modeManager.IsPositiveVisible = false;
                        }
                    }
                    _draggedItem = null;
                }
            }
        }
    }
}