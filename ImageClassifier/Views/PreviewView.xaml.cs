namespace ImageClassifier.Views;

public partial class PreviewView : ContentView
{
    private double _startX, _startY;
    private double _startTranslationX, _startTranslationY;
    public PreviewView()
	{
		InitializeComponent();
	}

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        var element = (VisualElement)sender;
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _startX = e.TotalX;
                _startY = e.TotalY;
                _startTranslationX = element.TranslationX;
                _startTranslationY = element.TranslationY;
                break;
            case GestureStatus.Running:
                double deltaX = e.TotalX - _startX;
                double deltaY = e.TotalY - _startY;
                element.TranslationX = _startTranslationX + deltaX;
                element.TranslationY = _startTranslationY + deltaY;
                break;
        }
    }
}