using System.Globalization;
using System.Windows.Data;

namespace AchievementTranslator.Views;

/// <summary>
/// Converts (percent 0-100, windowWidth) → pixel width for the progress bar rectangle.
/// </summary>
[ValueConversion(typeof(double), typeof(double))]
public class PercentToWidthConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2) return 0.0;
        if (values[0] is not double pct || values[1] is not double totalWidth) return 0.0;
        return Math.Max(0, totalWidth * pct / 100.0);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
