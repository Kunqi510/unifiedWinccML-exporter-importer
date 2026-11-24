using System;
using System.Globalization;
using System.Windows.Data;

namespace Siemens.Application.TiaPortal.WinccmlExporterImporter.UI.Converters;

/// <summary>
/// Converts a boolean value to its inverse.
/// </summary>
[ValueConversion(typeof(bool), typeof(bool))]
internal class InvertBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }
}
