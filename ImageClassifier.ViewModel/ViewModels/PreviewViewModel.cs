using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageClassifier.Core.Interfaces;
using ImageClassifier.ViewModel.Enums;
using System.Runtime.Versioning;

namespace ImageClassifier.ViewModel.ViewModels;

public partial class PreviewViewModel : ObservableObject
{
    public FileCollectionViewModel FileCollection { get; }
    public FullscreenViewModel Fullscreen { get; }
    public DragDropManagerViewModel DragDropManager { get; }
    public WorkflowViewModel Workflow { get; }
    public TrainMenuViewModel TrainMenu { get; }
    public ModeManagerViewModel ModeManager { get; }

    private readonly IFolderPicker _folderPicker;
    private readonly IDialogService _dialogService;
    private readonly IMediaPickerService _mediaPickerService;

    public PreviewViewModel(
        FileCollectionViewModel fileCollection,
        FullscreenViewModel fullscreen,
        DragDropManagerViewModel dragDropManager,
        WorkflowViewModel workflow,
        TrainMenuViewModel trainMenu,
        ModeManagerViewModel modeManager,
        IFolderPicker folderPicker,
        IDialogService dialogService,
        IMediaPickerService mediaPickerService)
    {
        FileCollection = fileCollection;
        Fullscreen = fullscreen;
        DragDropManager = dragDropManager;
        Workflow = workflow;
        TrainMenu = trainMenu;
        ModeManager = modeManager;

        _folderPicker = folderPicker;
        _dialogService = dialogService;
        _mediaPickerService = mediaPickerService;
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
    private async Task TrainTapped()
    {
        switch (ModeManager.CurrentMode)
        {
            case AppMode.Preview:
                TrainMenu.Show();
                break;
            case AppMode.Train:
                ModeManager.SelectMode(AppMode.Preview);
                break;
            case AppMode.Predict:
                ModeManager.SelectMode(AppMode.Processing);
                await Workflow.Predict();
                ModeManager.SelectMode(AppMode.Predict);
                break;
        }
    }

    [RelayCommand]
    private async Task PredictTapped()
    {
        switch (ModeManager.CurrentMode)
        {
            case AppMode.Preview:
                ModeManager.SelectMode(AppMode.Predict);
                break;
            case AppMode.Predict:
                ModeManager.SelectMode(AppMode.Preview);
                break;
            case AppMode.Train:
                ModeManager.SelectMode(AppMode.Processing);
                await Workflow.Train(TrainMenu.NewLabel);
                ModeManager.SelectMode(AppMode.Train);
                break;
        }
    }
}