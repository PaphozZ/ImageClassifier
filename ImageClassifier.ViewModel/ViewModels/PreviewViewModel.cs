using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageClassifier.Core.Interfaces;
using ImageClassifier.ViewModel.Extensions;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace ImageClassifier.ViewModel.ViewModels;

public partial class PreviewViewModel : ObservableObject
{
    public FileCollectionViewModel FileCollection { get; }
    public FullscreenViewModel Fullscreen { get; }

    private readonly IFolderPicker _folderPicker;
    private readonly IDialogService _dialogService;
    private readonly IMediaPickerService _mediaPickerService;

    public PreviewViewModel(
        FileCollectionViewModel fileCollection,
        FullscreenViewModel fullscreen,
        IFolderPicker folderPicker,
        IDialogService dialogService,
        IMediaPickerService mediaPickerService)
    {
        FileCollection = fileCollection;
        Fullscreen = fullscreen;
        _folderPicker = folderPicker;
        _dialogService = dialogService;
        _mediaPickerService = mediaPickerService;

        Task.Run(async () => await FileCollection.LoadSavedFilesAsync())
            .ContinueWith(task => 
            {
                LoadSavedFilesAsync()
                    .FireAndForget(ex => Debug.WriteLine($"Ошибка загрузки: {ex}"));
            });
    }

    private async Task LoadSavedFilesAsync()
    {
        foreach (var item in FileCollection.Files)
        {
            await LoadImageAsync(item);
        }
    }

    private async Task LoadImageAsync(ImageItemViewModel item)
    {
        var FileFullName = (!string.IsNullOrEmpty(item.FileName) && !string.IsNullOrEmpty(item.FilePath))
            ? Path.Combine(item.FilePath, item.FileName) : null;

        if (FileFullName != null)
        {
            var imageSource = ImageSource.FromFile(FileFullName);
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                item.FilePreview = imageSource;
            });
        }
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
            Fullscreen.ShowImage(file.FilePreview);
    }

    [RelayCommand]
    private void FullscreenTapped() => Fullscreen.Hide();
}