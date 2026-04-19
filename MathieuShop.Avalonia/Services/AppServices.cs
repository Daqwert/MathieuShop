using MathieuShop.Avalonia.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace MathieuShop.Avalonia.Services;

public sealed record AppOptions(string PostgreSqlConnectionString, int PageSize, bool FallbackToInMemory);

public sealed record DatabaseBootstrapResult(AppDbContext Context, string StatusMessage, bool UsingFallbackDatabase);

public static class AppOptionsLoader
{
    public static AppOptions Load(string basePath)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();

        var connectionString = configuration.GetConnectionString("PostgreSql")
            ?? "Host=localhost;Port=5432;Database=mathieu_shop;Username=karim007;Password=27082006";

        var pageSize = int.TryParse(configuration["App:PageSize"], out var configuredPageSize) ? configuredPageSize : 6;
        var fallbackToInMemory = bool.TryParse(configuration["App:FallbackToInMemory"], out var configuredFallback)
            ? configuredFallback
            : true;

        return new AppOptions(connectionString, pageSize, fallbackToInMemory);
    }
}

public static class PasswordHelper
{
    public static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    public static bool Verify(string rawValue, string hash)
    {
        return Hash(rawValue) == hash;
    }
}

public static class LastModifiedService
{
    public static DateTimeOffset Touch(DateTimeOffset? now = null)
    {
        return now ?? DateTimeOffset.UtcNow;
    }

    public static bool IsStale(DateTimeOffset updatedAt, int staleAfterDays, DateTimeOffset? now = null)
    {
        return ((now ?? DateTimeOffset.UtcNow) - updatedAt).TotalDays >= staleAfterDays;
    }

    public static string Describe(DateTimeOffset updatedAt, DateTimeOffset? now = null)
    {
        var current = now ?? DateTimeOffset.UtcNow;
        var delta = current - updatedAt;
        if (delta.TotalMinutes < 1)
        {
            return "обновлено только что";
        }

        if (delta.TotalHours < 1)
        {
            return $"обновлено {Math.Max(1, (int)delta.TotalMinutes)} мин. назад";
        }

        if (delta.TotalDays < 1)
        {
            return $"обновлено {(int)delta.TotalHours} ч. назад";
        }

        return $"обновлено {(int)delta.TotalDays} дн. назад";
    }
}

public static class DatabaseBootstrapper
{
    public static DatabaseBootstrapResult CreateContext(AppOptions options)
    {
        var postgresOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(options.PostgreSqlConnectionString)
            .EnableSensitiveDataLogging()
            .Options;

        try
        {
            var postgresContext = new AppDbContext(postgresOptions);
            postgresContext.Database.Migrate();
            DbSeeder.Seed(postgresContext);
            return new DatabaseBootstrapResult(postgresContext, "Подключение к PostgreSQL активно.", false);
        }
        catch (Exception exception)
        {
            if (!options.FallbackToInMemory)
            {
                throw;
            }

            var memoryOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("mathieu-shop-demo")
                .Options;
            var memoryContext = new AppDbContext(memoryOptions);
            memoryContext.Database.EnsureCreated();
            DbSeeder.Seed(memoryContext);
            return new DatabaseBootstrapResult(
                memoryContext,
                BuildFallbackMessage(exception),
                true);
        }
    }

    private static string BuildFallbackMessage(Exception exception)
    {
        return $"ВНИМАНИЕ: PostgreSQL недоступен. Приложение запущено в демонстрационном режиме, изменения не сохраняются в серверную базу. {DescribeDatabaseError(exception)}";
    }

    private static string DescribeDatabaseError(Exception exception)
    {
        var postgresException = FindException<PostgresException>(exception);
        if (postgresException is not null)
        {
            return postgresException.SqlState switch
            {
                "28P01" => "Не удалось подключиться к PostgreSQL: указан неверный пароль пользователя базы данных.",
                "28000" => "Не удалось подключиться к PostgreSQL: для указанного пользователя нет доступа.",
                "3D000" => "Не удалось подключиться к PostgreSQL: база данных не найдена.",
                _ => $"Не удалось подключиться к PostgreSQL. Код ошибки: {postgresException.SqlState}."
            };
        }

        if (FindException<SocketException>(exception) is not null)
        {
            return "Не удалось подключиться к PostgreSQL: сервер не отвечает по указанному адресу или порту.";
        }

        if (exception.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase)
            || exception.Message.Contains("timed out", StringComparison.OrdinalIgnoreCase))
        {
            return "Не удалось подключиться к PostgreSQL: превышено время ожидания ответа сервера.";
        }

        return "Проверьте строку подключения в appsettings.json, логин, пароль, имя базы данных и доступность сервера.";
    }

    private static TException? FindException<TException>(Exception exception)
        where TException : Exception
    {
        Exception? current = exception;
        while (current is not null)
        {
            if (current is TException typedException)
            {
                return typedException;
            }

            current = current.InnerException;
        }

        return null;
    }
}

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Users.Any())
        {
            return;
        }

        var now = new DateTimeOffset(2026, 4, 18, 14, 0, 0, TimeSpan.FromHours(3)).ToUniversalTime();

        var anime = new CollectionTheme
        {
            Name = "Аниме",
            Description = "Образы и коллекции по популярным тайтлам.",
            IsHoliday = false,
            ImagePath = "/Assets/Images/Кастом/Pr1.jpg"
        };
        var newYear = new CollectionTheme
        {
            Name = "Новый год",
            Description = "Праздничные услуги и подарочные наборы.",
            IsHoliday = true,
            ImagePath = "/Assets/Images/Кастом/Pr10.jpg"
        };
        var halloween = new CollectionTheme
        {
            Name = "Хэллоуин",
            Description = "Мрачные тематические работы и аксессуары.",
            IsHoliday = true,
            ImagePath = "/Assets/Images/Косплей/KL4.jpg"
        };
        var cyberpunk = new CollectionTheme
        {
            Name = "Киберпанк",
            Description = "Неоновые образы и цифровые детали.",
            IsHoliday = false,
            ImagePath = "/Assets/Images/Косплей/KL6.jpg"
        };
        var noir = new CollectionTheme
        {
            Name = "Нуар",
            Description = "Тёмная палитра, ретро и кинематографичность.",
            IsHoliday = false,
            ImagePath = "/Assets/Images/Кастом/Pr4.jpg"
        };

        db.CollectionThemes.AddRange(anime, newYear, halloween, cyberpunk, noir);

        var admin = new User
        {
            FullName = "София Дымова",
            Login = "admin",
            Email = "admin@mathieu.local",
            Phone = "+7 900 111-11-11",
            PasswordHash = PasswordHelper.Hash("demo123"),
            Role = UserRole.Admin,
            Balance = 0,
            CreatedAt = now,
            UpdatedAt = now
        };
        var moderator = new User
        {
            FullName = "Марк Роуэн",
            Login = "moderator",
            Email = "moderator@mathieu.local",
            Phone = "+7 900 111-22-22",
            PasswordHash = PasswordHelper.Hash("demo123"),
            Role = UserRole.Moderator,
            Balance = 0,
            CreatedAt = now,
            UpdatedAt = now
        };
        var master1 = new User
        {
            FullName = "Арина Вега",
            Login = "master1",
            Email = "master1@mathieu.local",
            Phone = "+7 900 111-33-33",
            PasswordHash = PasswordHelper.Hash("demo123"),
            Role = UserRole.Master,
            Balance = 0,
            CreatedAt = now,
            UpdatedAt = now
        };
        var master2 = new User
        {
            FullName = "Лев Ларсен",
            Login = "master2",
            Email = "master2@mathieu.local",
            Phone = "+7 900 111-44-44",
            PasswordHash = PasswordHelper.Hash("demo123"),
            Role = UserRole.Master,
            Balance = 0,
            CreatedAt = now,
            UpdatedAt = now
        };
        var client1 = new User
        {
            FullName = "Ева Найт",
            Login = "client",
            Email = "client@mathieu.local",
            Phone = "+7 900 111-55-55",
            PasswordHash = PasswordHelper.Hash("demo123"),
            Role = UserRole.Client,
            Balance = 6000,
            CreatedAt = now,
            UpdatedAt = now
        };
        var client2 = new User
        {
            FullName = "Дан Миронов",
            Login = "client2",
            Email = "client2@mathieu.local",
            Phone = "+7 900 111-66-66",
            PasswordHash = PasswordHelper.Hash("demo123"),
            Role = UserRole.Client,
            Balance = 2500,
            CreatedAt = now,
            UpdatedAt = now
        };

        var moderatorEmployee = new Employee
        {
            User = moderator,
            Specialty = "Куратор коллекций",
            QualificationLevel = 4,
            About = "Ведёт коллекции и следит за связями услуг с мастерами.",
            HireDate = now.AddMonths(-16)
        };
        var employee1 = new Employee
        {
            User = master1,
            Specialty = "Кастом и окрашивание",
            QualificationLevel = 3,
            About = "Специализируется на праздничных и нуарных образах.",
            HireDate = now.AddMonths(-10)
        };
        var employee2 = new Employee
        {
            User = master2,
            Specialty = "Косплей и декор",
            QualificationLevel = 5,
            About = "Работает с деталями костюмов и сложным гримом.",
            HireDate = now.AddMonths(-22)
        };

        var services = new List<ServiceItem>
        {
            new()
            {
                Name = "Кастомизация базового образа",
                Description = "Подбор палитры, аксессуаров и мелкого декора под образ клиента.",
                Price = 3200,
                DurationMinutes = 90,
                ImagePath = "/Assets/Images/Кастом/Pr2.jpg",
                IsHoliday = false,
                CreatedAt = now.AddDays(-14),
                LastModifiedAt = now.AddDays(-2),
                CollectionTheme = anime
            },
            new()
            {
                Name = "Сборка новогоднего комплекта",
                Description = "Композиция из аксессуаров, цвета и акцентных элементов под зимнюю коллекцию.",
                Price = 4100,
                DurationMinutes = 120,
                ImagePath = "/Assets/Images/Кастом/Pr10.jpg",
                IsHoliday = true,
                CreatedAt = now.AddDays(-20),
                LastModifiedAt = now.AddDays(-4),
                CollectionTheme = newYear
            },
            new()
            {
                Name = "Хэллоуин-грим премиум",
                Description = "Подготовка полноценного мрачного образа с гримом и текстурными материалами.",
                Price = 5600,
                DurationMinutes = 150,
                ImagePath = "/Assets/Images/Косплей/KL4.jpg",
                IsHoliday = true,
                CreatedAt = now.AddDays(-30),
                LastModifiedAt = now.AddHours(-8),
                CollectionTheme = halloween
            },
            new()
            {
                Name = "Киберпанк-детализация",
                Description = "Неоновая детализация костюма, подбор подсветки и механических аксессуаров.",
                Price = 6200,
                DurationMinutes = 180,
                ImagePath = "/Assets/Images/Косплей/KL6.jpg",
                IsHoliday = false,
                CreatedAt = now.AddDays(-40),
                LastModifiedAt = now.AddDays(-1),
                CollectionTheme = cyberpunk
            },
            new()
            {
                Name = "Нуарный макияж и стилизация",
                Description = "Контрастный грим и стилистика в чёрно-золотой палитре.",
                Price = 3500,
                DurationMinutes = 110,
                ImagePath = "/Assets/Images/Кастом/Pr4.jpg",
                IsHoliday = false,
                CreatedAt = now.AddDays(-15),
                LastModifiedAt = now.AddDays(-3),
                CollectionTheme = noir
            },
            new()
            {
                Name = "Косплей-пакет старт",
                Description = "Быстрая адаптация костюма под мероприятие с фиксацией мелких элементов.",
                Price = 2800,
                DurationMinutes = 80,
                ImagePath = "/Assets/Images/Косплей/KL1.jpg",
                IsHoliday = false,
                CreatedAt = now.AddDays(-17),
                LastModifiedAt = now.AddDays(-7),
                CollectionTheme = anime
            },
            new()
            {
                Name = "Праздничный подарочный сет",
                Description = "Персональный сет под событие с упаковкой, цветом и коллекционной карточкой.",
                Price = 3900,
                DurationMinutes = 95,
                ImagePath = "/Assets/Images/Кастом/Pr12.jpg",
                IsHoliday = true,
                CreatedAt = now.AddDays(-12),
                LastModifiedAt = now.AddDays(-2),
                CollectionTheme = newYear
            },
            new()
            {
                Name = "Нуарный фотосет-образ",
                Description = "Подготовка стилизации для съёмки с акцентом на аксессуары и композицию.",
                Price = 4700,
                DurationMinutes = 140,
                ImagePath = "/Assets/Images/Кастом/Pr6.jpg",
                IsHoliday = false,
                CreatedAt = now.AddDays(-10),
                LastModifiedAt = now.AddMinutes(-30),
                CollectionTheme = noir
            }
        };

        db.Users.AddRange(admin, moderator, master1, master2, client1, client2);
        db.Employees.AddRange(moderatorEmployee, employee1, employee2);
        db.ServiceItems.AddRange(services);
        db.SaveChanges();

        db.ServiceAssignments.AddRange(
            new ServiceAssignment { Employee = employee1, ServiceItem = services[0], AttachedAt = now.AddDays(-12) },
            new ServiceAssignment { Employee = employee1, ServiceItem = services[1], AttachedAt = now.AddDays(-11) },
            new ServiceAssignment { Employee = employee1, ServiceItem = services[4], AttachedAt = now.AddDays(-9) },
            new ServiceAssignment { Employee = employee1, ServiceItem = services[6], AttachedAt = now.AddDays(-7) },
            new ServiceAssignment { Employee = employee2, ServiceItem = services[2], AttachedAt = now.AddDays(-13) },
            new ServiceAssignment { Employee = employee2, ServiceItem = services[3], AttachedAt = now.AddDays(-13) },
            new ServiceAssignment { Employee = employee2, ServiceItem = services[5], AttachedAt = now.AddDays(-10) },
            new ServiceAssignment { Employee = employee2, ServiceItem = services[7], AttachedAt = now.AddDays(-8) });

        var booking1 = new Booking
        {
            Customer = client1,
            Employee = employee1,
            ServiceItem = services[0],
            ScheduledAt = now.AddDays(2).Date.AddHours(16),
            QueueNumber = 1,
            Status = BookingStatus.Planned,
            Notes = "Нужна палитра в тёплых тонах.",
            CreatedAt = now.AddDays(-1)
        };
        var booking2 = new Booking
        {
            Customer = client1,
            Employee = employee2,
            ServiceItem = services[2],
            ScheduledAt = now.AddDays(3).Date.AddHours(18),
            QueueNumber = 1,
            Status = BookingStatus.Planned,
            Notes = "Образ на вечерний ивент.",
            CreatedAt = now.AddHours(-12)
        };
        var booking3 = new Booking
        {
            Customer = client2,
            Employee = employee2,
            ServiceItem = services[5],
            ScheduledAt = now.AddDays(-4).Date.AddHours(15),
            QueueNumber = 2,
            Status = BookingStatus.Completed,
            Notes = "Нужна экспресс-подготовка.",
            CreatedAt = now.AddDays(-6)
        };

        db.Bookings.AddRange(booking1, booking2, booking3);

        db.Reviews.AddRange(
            new Review
            {
                Customer = client2,
                Booking = booking3,
                Employee = employee2,
                ServiceItem = services[5],
                Rating = 5,
                Comment = "Быстро, аккуратно и очень удобно по времени.",
                CreatedAt = now.AddDays(-3)
            },
            new Review
            {
                Customer = client1,
                Employee = employee1,
                ServiceItem = services[4],
                Rating = 4,
                Comment = "Понравилась стилистика и подбор аксессуаров.",
                CreatedAt = now.AddDays(-2)
            });

        db.TopUpTransactions.AddRange(
            new TopUpTransaction
            {
                User = client1,
                Amount = 3000,
                CardMask = "2200 **** **** 5678",
                CreatedAt = now.AddDays(-5)
            },
            new TopUpTransaction
            {
                User = client2,
                Amount = 1500,
                CardMask = "2200 **** **** 7788",
                CreatedAt = now.AddDays(-2)
            });

        db.QualificationRequests.Add(
            new QualificationRequest
            {
                Employee = employee1,
                DesiredLevel = 4,
                Comment = "Хочу расширить доступ к сложным праздничным пакетам.",
                Status = QualificationRequestStatus.Pending,
                CreatedAt = now.AddDays(-1)
            });

        db.SaveChanges();
    }
}
