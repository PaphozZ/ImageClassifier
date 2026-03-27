using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageClassifier.Core.Enums;
using ImageClassifier.ViewModel.Enums;
using ImageClassifier.ViewModel.ViewModels;

public partial class DragDropManagerViewModel : ObservableObject
{
    private readonly FileCollectionViewModel _fileCollection;
    private readonly WorkflowViewModel _workflowManager;

    private ImageItemViewModel? _draggedItem;

    public DragDropManagerViewModel(
        FileCollectionViewModel fileCollection, 
        WorkflowViewModel workflowManager)
    {
        _fileCollection = fileCollection;
        _workflowManager = workflowManager;
    }

    [RelayCommand]
    private void DragStarting(ImageItemViewModel item) => _draggedItem = item;

    [RelayCommand]
    private void DropToPositiveItems()
    {
        if (_draggedItem != null && !_draggedItem.IsDeleted && !_fileCollection.PositiveItems.Contains(_draggedItem))
        {
            _fileCollection.PositiveItems.Add(_draggedItem);
            if (_fileCollection.NegativeItems.Contains(_draggedItem))
                _fileCollection.NegativeItems.Remove(_draggedItem);
            _draggedItem.DatasetClass = DatasetClass.Positive;
            _draggedItem = null;

            if (_workflowManager.CurrentMode == AppMode.Predict)
            {
                _workflowManager.IsNegativeVisible = false;
            }
        }
    }

    [RelayCommand]
    private async Task DropToNegativeItems()
    {
        if (_draggedItem != null && !_draggedItem.IsDeleted && !_fileCollection.NegativeItems.Contains(_draggedItem))
        {
            if (_workflowManager.CurrentMode == AppMode.Preview)
            {
                await _fileCollection.RemoveFileAsync(_draggedItem);
            }
            else
            {
                _fileCollection.NegativeItems.Add(_draggedItem);
                if (_fileCollection.PositiveItems.Contains(_draggedItem))
                    _fileCollection.PositiveItems.Remove(_draggedItem);
                _draggedItem.DatasetClass = DatasetClass.Negative;

                if (_workflowManager.CurrentMode == AppMode.Predict)
                {
                    _workflowManager.IsPositiveVisible = false;
                }
            }
            _draggedItem = null;
        }
    }

    [RelayCommand]
    private void RemoveLastPositiveItem()
    {
        var lastItem = _fileCollection.PositiveItems.LastOrDefault();
        if (lastItem != null)
        {
            lastItem.DatasetClass = DatasetClass.None;
            _fileCollection.PositiveItems.Remove(lastItem);

            if (_workflowManager.CurrentMode == AppMode.Predict && _fileCollection.PositiveItems.Count == 0)
            {
                _workflowManager.IsNegativeVisible = true;
            }
        }
    }

    [RelayCommand]
    private void RemoveLastNegativeItem()
    {
        var lastItem = _fileCollection.NegativeItems.LastOrDefault();
        if (lastItem != null)
        {
            lastItem.DatasetClass = DatasetClass.None;
            _fileCollection.NegativeItems.Remove(lastItem);

            if (_workflowManager.CurrentMode == AppMode.Predict && _fileCollection.NegativeItems.Count == 0)
            {
                _workflowManager.IsPositiveVisible = true;
            }
        }
    }
}