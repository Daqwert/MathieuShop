using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using MathieuShop.Avalonia.Services;
using MathieuShop.Avalonia.ViewModels;
using MathieuShop.Avalonia.Views;
using System.Linq;

namespace MathieuShop.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();
            var options = AppOptionsLoader.Load(AppContext.BaseDirectory);
            var bootstrap = DatabaseBootstrapper.CreateContext(options);
            desktop.Exit += (_, _) => bootstrap.Context.Dispose();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(bootstrap.Context, options, bootstrap.StatusMessage, bootstrap.UsingFallbackDatabase),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
