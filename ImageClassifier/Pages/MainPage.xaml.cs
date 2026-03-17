using ImageClassifier.ViewModel.ViewModels;

namespace ImageClassifier.Pages;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
