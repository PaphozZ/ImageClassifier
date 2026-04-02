namespace ImageClassifier.Views;

    public partial class ImageListView : ContentView
    {
        private int _currentSpan = -1;

        public ImageListView()
        {
            InitializeComponent();
            PreviewGrid.SizeChanged += OnPreviewGridSizeChanged;
        }

        private void OnPreviewGridSizeChanged(object? sender, EventArgs e)
        {
            if (PreviewGrid.Width <= 0)
                return;

            int newSpan = (int)(PreviewGrid.Width / 360);
            if (newSpan < 1) newSpan = 1;

            if (newSpan == _currentSpan)
                return;

            _currentSpan = newSpan;
            DynamicGridLayout.Span = newSpan;
        }
    }