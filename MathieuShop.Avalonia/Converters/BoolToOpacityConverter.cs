using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MathieuShop.Avalonia.Converters;

public sealed class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? 1d : 0.55d;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}
