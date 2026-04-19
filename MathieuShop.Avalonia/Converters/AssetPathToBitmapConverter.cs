using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Concurrent;
using System.Globalization;

namespace MathieuShop.Avalonia.Converters;

public sealed class AssetPathToBitmapConverter : IValueConverter
{
    private const string AssetBaseUri = "avares://MathieuShop.Avalonia";
    private const string FallbackAssetPath = "/Assets/Logo.png";
    private static readonly ConcurrentDictionary<string, Bitmap> Cache = new(StringComparer.OrdinalIgnoreCase);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var path = value as string;
        if (string.IsNullOrWhiteSpace(path))
        {
            return TryLoadBitmap(FallbackAssetPath);
        }

        return TryLoadBitmap(path) ?? TryLoadBitmap(FallbackAssetPath);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }

    private static Bitmap? TryLoadBitmap(string path)
    {
        var uri = BuildAssetUri(path);
        if (uri is null)
        {
            return null;
        }

        return Cache.GetOrAdd(uri.AbsoluteUri, static key =>
        {
            using var stream = AssetLoader.Open(new Uri(key));
            return new Bitmap(stream);
        });
    }

    private static Uri? BuildAssetUri(string path)
    {
        var normalizedPath = path.Trim().Replace('\\', '/');
        if (string.IsNullOrWhiteSpace(normalizedPath))
        {
            return null;
        }

        if (normalizedPath.StartsWith("avares://", StringComparison.OrdinalIgnoreCase)
            && Uri.TryCreate(normalizedPath, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri;
        }

        if (!normalizedPath.StartsWith('/'))
        {
            normalizedPath = "/" + normalizedPath.TrimStart('/');
        }

        return Uri.TryCreate($"{AssetBaseUri}{normalizedPath}", UriKind.Absolute, out var assetUri)
            ? assetUri
            : null;
    }
}
