using System.Globalization;

namespace ImageClassifier.Converters
{
    public class WidthToColumnCountConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double width && parameter is string param && double.TryParse(param, out double itemWidth) && itemWidth > 0)
            {
                if (width <= 0) return 1;
                int columns = (int)(width / itemWidth);
                return Math.Max(1, columns);
            }
            return 1;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
