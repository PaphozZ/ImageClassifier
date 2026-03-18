using ImageClassifier.ViewModel.ViewModels;

namespace ImageClassifier.Selectors
{
    public class ViewTemplateSelector : DataTemplateSelector
    {
        public required DataTemplate SampleViewTemplate { get; set; }
        public required DataTemplate PreviewViewTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return item switch
            {
                SampleViewModel => SampleViewTemplate,
                PreviewViewModel => PreviewViewTemplate,
                _ => throw new NotSupportedException($"Unknown view model type: {item?.GetType()}")
            };
        }
    }
}