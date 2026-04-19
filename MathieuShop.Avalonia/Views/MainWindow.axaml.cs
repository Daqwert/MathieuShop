using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MathieuShop.Avalonia.ViewModels;

namespace MathieuShop.Avalonia.Views;

public partial class MainWindow : Window
{
    private const double PreferredWidth = 1366;
    private const double PreferredHeight = 820;
    private const double MinimumWorkingMargin = 24;

    public MainWindow()
    {
        InitializeComponent();
        Opened += MainWindow_Opened;
        Closing += MainWindow_Closing;
    }

    private void MainWindow_Opened(object? sender, EventArgs e)
    {
        var screen = Screens.ScreenFromWindow(this) ?? Screens.Primary;
        if (screen is null)
        {
            return;
        }

        var scaling = screen.Scaling <= 0 ? 1d : screen.Scaling;
        var workArea = screen.WorkingArea;
        var allowedWidth = Math.Max(640d, (workArea.Width / scaling) - MinimumWorkingMargin);
        var allowedHeight = Math.Max(480d, (workArea.Height / scaling) - MinimumWorkingMargin);
        var safeMinWidth = Math.Min(MinWidth, allowedWidth);
        var safeMinHeight = Math.Min(MinHeight, allowedHeight);

        MinWidth = safeMinWidth;
        MinHeight = safeMinHeight;
        Width = Math.Max(safeMinWidth, Math.Min(PreferredWidth, allowedWidth));
        Height = Math.Max(safeMinHeight, Math.Min(PreferredHeight, allowedHeight));
        MaxWidth = allowedWidth;
        MaxHeight = allowedHeight;

        var pixelWidth = (int)Math.Round(Width * scaling);
        var pixelHeight = (int)Math.Round(Height * scaling);

        Position = new PixelPoint(
            workArea.X + Math.Max(0, (workArea.Width - pixelWidth) / 2),
            workArea.Y + Math.Max(0, (workArea.Height - pixelHeight) / 2));
    }

    private void Minimize_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.RequestClose();
        }
    }

    private void CancelClose_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.ShowExitDialog = false;
        }
    }

    private void ConfirmClose_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.AllowShutdown = true;
            Close();
        }
    }

    private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel && !viewModel.AllowShutdown)
        {
            e.Cancel = true;
            viewModel.RequestClose();
        }
    }

    private void TitleBar_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }
}
