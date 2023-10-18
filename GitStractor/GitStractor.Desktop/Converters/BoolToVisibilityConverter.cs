using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GitStractor.Desktop.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public Visibility ValueIfTrue { get; set; } = Visibility.Visible;
    public Visibility ValueIfFalse { get; set; } = Visibility.Collapsed;
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            return (bool)value
                ? ValueIfTrue 
                : ValueIfFalse;
        }
        catch (InvalidCastException)
        {
            return ValueIfFalse;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}