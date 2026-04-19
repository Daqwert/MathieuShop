using MathieuShop.Avalonia.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MathieuShop.Avalonia.Data;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var basePath = File.Exists(Path.Combine(currentDirectory, "appsettings.json"))
            ? currentDirectory
            : Path.Combine(currentDirectory, "MathieuShop.Avalonia");

        var options = AppOptionsLoader.Load(basePath);
        var builder = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(options.PostgreSqlConnectionString);

        return new AppDbContext(builder.Options);
    }
}
