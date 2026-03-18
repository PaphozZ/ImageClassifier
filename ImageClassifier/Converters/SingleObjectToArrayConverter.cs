using System.Globalization;

namespace ImageClassifier.Converters
{
    public class SingleObjectToArrayConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is null ? value : new object[] { value };

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}