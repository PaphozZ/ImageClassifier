using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageClassifier.Core.Interfaces;
using System.Runtime.Versioning;

namespace ImageClassifier.ViewModel.ViewModels;

public partial class PreviewViewModel : ObservableObject
{
    public FileCollectionViewModel FileCollection { get; }
    public FullscreenViewModel Fullscreen { get; }
    public WorkflowViewModel Workflow { get; }
    public DragDropManagerViewModel DragDropManager { get; }

    private readonly IFolderPicker _folderPicker;
    private readonly IDialogService _dialogService;
    private readonly IMediaPickerService _mediaPickerService;
    private readonly ITaskCommanderService _taskCommanderService;

    [ObservableProperty]
    private ImageItemViewModel? _selectedImageItem;

    public PreviewViewModel(
        FileCollectionViewModel fileCollection,
        FullscreenViewModel fullscreen,
        WorkflowViewModel modeManager,
        DragDropManagerViewModel dragDropManager,
        IFolderPicker folderPicker,
        IDialogService dialogService,
        IMediaPickerService mediaPickerService,
        ITaskCommanderService taskCommander)
    {
        FileCollection = fileCollection;
        Fullscreen = fullscreen;
        Workflow = modeManager;
        DragDropManager = dragDropManager;

        _folderPicker = folderPicker;
        _dialogService = dialogService;
        _mediaPickerService = mediaPickerService;
        _taskCommanderService = taskCommander;

        _taskCommanderService.AddTask(FileCollection.LoadSavedFilesAsync);
    }


    [RelayCommand]
    private async Task PickImageAsync()
    {
        try
        {
            var model = await _mediaPickerService.PickImageAsync();
            if (model != null)
            {
                await FileCollection.AddFileAsync(model);
            }
        }
        catch
        {
            await _dialogService.DisplayAlert("Ошибка", "Не удалось загрузить файл", "OK");
        }
    }

    [RelayCommand]
    [SupportedOSPlatform("windows")]
    private async Task PickFolderAsync()
    {
        try
        {
            var result = await _folderPicker.PickAsync(default);
            if (result.IsSuccessful)
            {
                await FileCollection.AddFilesFromFolderAsync(result.Folder.Path);
            }
        }
        catch
        {
            await _dialogService.DisplayAlert("Ошибка", "Не удалось загрузить файлы", "OK");
        }
    }

    [RelayCommand]
    private void SelectFile(ImageItemViewModel file)
    {
        SelectedImageItem = file;
        if (file.FilePreview != null)
            _taskCommanderService.AddTask(() => Fullscreen.ShowImageAsync(file), true);
    }
}