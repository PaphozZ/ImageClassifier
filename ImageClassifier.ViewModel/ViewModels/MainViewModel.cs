using CommunityToolkit.Mvvm.ComponentModel;

namespace ImageClassifier.ViewModel.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private object _currentView;

        public MainViewModel(PreviewViewModel viewModel)
        {
            CurrentView = viewModel;
        }
    }
}
