using MathieuShop.Avalonia.Services;

namespace MathieuShop.Tests;

public sealed class LastModifiedServiceTests
{
    [Fact]
    public void Touch_ReturnsProvidedTimestamp()
    {
        var expected = new DateTimeOffset(2026, 4, 18, 15, 0, 0, TimeSpan.FromHours(3));

        var result = LastModifiedService.Touch(expected);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Describe_ForRecentUpdate_ReturnsMinutesText()
    {
        var now = new DateTimeOffset(2026, 4, 18, 15, 0, 0, TimeSpan.FromHours(3));
        var updatedAt = now.AddMinutes(-12);

        var result = LastModifiedService.Describe(updatedAt, now);

        Assert.Equal("обновлено 12 мин. назад", result);
    }

    [Fact]
    public void Describe_ForDayOldUpdate_ReturnsDaysText()
    {
        var now = new DateTimeOffset(2026, 4, 18, 15, 0, 0, TimeSpan.FromHours(3));
        var updatedAt = now.AddDays(-3).AddHours(-2);

        var result = LastModifiedService.Describe(updatedAt, now);

        Assert.Equal("обновлено 3 дн. назад", result);
    }

    [Theory]
    [InlineData(7, true)]
    [InlineData(6, false)]
    public void IsStale_UsesThresholdInDays(int daysBack, bool expected)
    {
        var now = new DateTimeOffset(2026, 4, 18, 15, 0, 0, TimeSpan.FromHours(3));
        var updatedAt = now.AddDays(-daysBack);

        var result = LastModifiedService.IsStale(updatedAt, 7, now);

        Assert.Equal(expected, result);
    }
}
