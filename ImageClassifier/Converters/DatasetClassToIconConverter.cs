using ImageClassifier.Core.Enums;
using System.Globalization;

namespace ImageClassifier.Converters
{
    public class DatasetClassToIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                DatasetClass.Positive => "check_circle_green.png",
                DatasetClass.Negative => "cancel_red.png",
                _ => null
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}