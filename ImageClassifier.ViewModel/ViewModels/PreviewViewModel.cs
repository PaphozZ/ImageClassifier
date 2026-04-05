using CommunityToolkit.Mvvm.ComponentModel;

namespace ImageClassifier.ViewModel.ViewModels;

public partial class PreviewViewModel : ObservableObject
{
    public FileCollectionViewModel FileCollection { get; }
    public FullscreenViewModel Fullscreen { get; }
    public DragDropManagerViewModel DragDropManager { get; }
    public WorkflowViewModel Workflow { get; }
    public TrainMenuViewModel TrainMenu { get; }
    public ModeManagerViewModel ModeManager { get; }
    public PredictMenuViewModel PredictMenu { get; }
    public SidePanelViewModel SidePanel { get; }

    public PreviewViewModel(
        FileCollectionViewModel fileCollection,
        FullscreenViewModel fullscreen,
        DragDropManagerViewModel dragDropManager,
        WorkflowViewModel workflow,
        TrainMenuViewModel trainMenu,
        ModeManagerViewModel modeManager,
        PredictMenuViewModel predictMenu,
        SidePanelViewModel sidePanel)
    {
        FileCollection = fileCollection;
        Fullscreen = fullscreen;
        DragDropManager = dragDropManager;
        Workflow = workflow;
        TrainMenu = trainMenu;
        ModeManager = modeManager;
        PredictMenu = predictMenu;
        SidePanel = sidePanel;
    }
}