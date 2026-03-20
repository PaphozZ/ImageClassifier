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

    private readonly IFolderPicker _folderPicker;
    private readonly IDialogService _dialogService;
    private readonly IMediaPickerService _mediaPickerService;
    private readonly ITaskCommanderService _taskCommanderService;

    public PreviewViewModel(
        FileCollectionViewModel fileCollection,
        FullscreenViewModel fullscreen,
        IFolderPicker folderPicker,
        IDialogService dialogService,
        IMediaPickerService mediaPickerService,
        ITaskCommanderService taskCommander)
    {
        FileCollection = fileCollection;
        Fullscreen = fullscreen;
        _folderPicker = folderPicker;
        _dialogService = dialogService;
        _mediaPickerService = mediaPickerService;
        _taskCommanderService = taskCommander;

        _taskCommanderService.AddTask(() => FileCollection.LoadSavedFilesAsync());
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
        if (file.FilePreview != null)
            _taskCommanderService.AddTask(() => Fullscreen.ShowImageAsync(file), true);
    }

    [RelayCommand]
    private void FullscreenTapped() => Fullscreen.Hide();
}