using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathieuShop.Avalonia.Data;
using MathieuShop.Avalonia.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace MathieuShop.Avalonia.ViewModels;

public enum AppSection
{
    Home,
    Catalog,
    Bookings,
    Balance,
    Reviews,
    Admin,
    Moderator,
    Master
}

public sealed class ThemeCardItem
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsHoliday { get; init; }
    public string ImagePath { get; init; } = string.Empty;

    public override string ToString() => Name;
}

public sealed class EmployeeChoice
{
    public int Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public int QualificationLevel { get; init; }

    public override string ToString() => $"{DisplayName} (ур. {QualificationLevel})";
}

public sealed class ServiceCatalogItem
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ThemeName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int DurationMinutes { get; init; }
    public string ImagePath { get; init; } = string.Empty;
    public bool IsHoliday { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
    public string LastModifiedText { get; init; } = string.Empty;
    public List<EmployeeChoice> Masters { get; init; } = [];
}

public sealed class BookingSlotOption
{
    public string Label { get; init; } = string.Empty;
    public DateTimeOffset Value { get; init; }

    public override string ToString() => Label;
}

public sealed class BookingItem
{
    public int Id { get; init; }
    public int ServiceId { get; init; }
    public int EmployeeId { get; init; }
    public string ServiceName { get; init; } = string.Empty;
    public string MasterName { get; init; } = string.Empty;
    public DateTimeOffset ScheduledAt { get; init; }
    public int QueueNumber { get; init; }
    public string StatusText { get; init; } = string.Empty;
    public string ImagePath { get; init; } = string.Empty;
    public bool CanReview { get; init; }
}

public sealed class ReviewItem
{
    public string CustomerName { get; init; } = string.Empty;
    public string ServiceName { get; init; } = string.Empty;
    public string MasterName { get; init; } = string.Empty;
    public int Rating { get; init; }
    public string Comment { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed class ServiceEditorItem
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ThemeName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string ImagePath { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
}

public sealed class UserEditorItem
{
    public int Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Login { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public decimal Balance { get; init; }
    public bool IsBlocked { get; init; }
}

public sealed class QualificationRequestItem
{
    public int Id { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public int DesiredLevel { get; init; }
    public string Comment { get; init; } = string.Empty;
    public string StatusText { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly AppDbContext _db;
    private readonly AppOptions _options;
    private List<ServiceCatalogItem> _catalogSource = [];
    private bool _suspendServiceSelectionUpdate;

    public MainWindowViewModel(AppDbContext db, AppOptions options, string startupMessage, bool usingFallbackDatabase)
    {
        _db = db;
        _options = options;
        IsUsingFallbackDatabase = usingFallbackDatabase;
        StatusMessage = startupMessage;
        Login = "client";
        Password = "demo123";
        TopUpAmount = "1500";
        TopUpCardNumber = "2200 7000 1234 5678";
        ManagedServicePrice = "3200";
        ManagedServiceDuration = "90";
        EditedUserBalance = "0";
        ReviewComment = "Очень понравился результат и аккуратная работа.";
        QualificationRequestComment = "Готов брать более сложные праздничные заказы.";

        AvailableRatings = new ObservableCollection<int>([5, 4, 3, 2, 1]);
        ThemeCards = new ObservableCollection<ThemeCardItem>();
        HomeThemeProducts = new ObservableCollection<ServiceCatalogItem>();
        CatalogItems = new ObservableCollection<ServiceCatalogItem>();
        BookingSlots = new ObservableCollection<BookingSlotOption>();
        ClientBookings = new ObservableCollection<BookingItem>();
        ReviewFeed = new ObservableCollection<ReviewItem>();
        ManagedServices = new ObservableCollection<ServiceEditorItem>();
        ThemeOptions = new ObservableCollection<ThemeCardItem>();
        AvailableMasters = new ObservableCollection<EmployeeChoice>();
        BoundMasters = new ObservableCollection<EmployeeChoice>();
        ManagedUsers = new ObservableCollection<UserEditorItem>();
        MasterBookings = new ObservableCollection<BookingItem>();
        QualificationRequests = new ObservableCollection<QualificationRequestItem>();
        EmployeeDirectory = new ObservableCollection<EmployeeChoice>();
        AvailableEmployeeRoles = new ObservableCollection<UserRole>([UserRole.Master, UserRole.Moderator, UserRole.Admin]);

        BuildBookingSlots();
        LoadThemeCards();
        ReloadData();
    }

    public ObservableCollection<int> AvailableRatings { get; }
    public ObservableCollection<ThemeCardItem> ThemeCards { get; }
    public ObservableCollection<ServiceCatalogItem> HomeThemeProducts { get; }
    public ObservableCollection<ServiceCatalogItem> CatalogItems { get; }
    public ObservableCollection<BookingSlotOption> BookingSlots { get; }
    public ObservableCollection<BookingItem> ClientBookings { get; }
    public ObservableCollection<ReviewItem> ReviewFeed { get; }
    public ObservableCollection<ServiceEditorItem> ManagedServices { get; }
    public ObservableCollection<ThemeCardItem> ThemeOptions { get; }
    public ObservableCollection<EmployeeChoice> AvailableMasters { get; }
    public ObservableCollection<EmployeeChoice> BoundMasters { get; }
    public ObservableCollection<UserEditorItem> ManagedUsers { get; }
    public ObservableCollection<BookingItem> MasterBookings { get; }
    public ObservableCollection<QualificationRequestItem> QualificationRequests { get; }
    public ObservableCollection<EmployeeChoice> EmployeeDirectory { get; }
    public ObservableCollection<UserRole> AvailableEmployeeRoles { get; }

    [ObservableProperty]
    private ThemeCardItem? selectedHomeTheme;

    [ObservableProperty]
    private ThemeCardItem? selectedCatalogTheme;

    [ObservableProperty]
    private string catalogSearchText = string.Empty;

    [ObservableProperty]
    private string login = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool showRegistration;

    [ObservableProperty]
    private string registerFullName = string.Empty;

    [ObservableProperty]
    private string registerLogin = string.Empty;

    [ObservableProperty]
    private string registerEmail = string.Empty;

    [ObservableProperty]
    private string registerPhone = string.Empty;

    [ObservableProperty]
    private string registerPassword = string.Empty;

    [ObservableProperty]
    private User? currentUser;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isUsingFallbackDatabase;

    [ObservableProperty]
    private AppSection currentSection = AppSection.Home;

    [ObservableProperty]
    private bool showExitDialog;

    [ObservableProperty]
    private bool allowShutdown;

    [ObservableProperty]
    private bool sortCatalogAscending = true;

    [ObservableProperty]
    private int currentCatalogPage = 1;

    [ObservableProperty]
    private ServiceCatalogItem? selectedCatalogItem;

    [ObservableProperty]
    private EmployeeChoice? selectedCatalogMaster;

    [ObservableProperty]
    private BookingSlotOption? selectedBookingSlot;

    [ObservableProperty]
    private string bookingNotes = string.Empty;

    [ObservableProperty]
    private string topUpAmount = string.Empty;

    [ObservableProperty]
    private string topUpCardNumber = string.Empty;

    [ObservableProperty]
    private BookingItem? selectedBookingForReview;

    [ObservableProperty]
    private int selectedRating = 5;

    [ObservableProperty]
    private string reviewComment = string.Empty;

    [ObservableProperty]
    private ServiceEditorItem? selectedManagedService;

    [ObservableProperty]
    private string managedServiceName = string.Empty;

    [ObservableProperty]
    private string managedServiceDescription = string.Empty;

    [ObservableProperty]
    private string managedServicePrice = string.Empty;

    [ObservableProperty]
    private string managedServiceDuration = string.Empty;

    [ObservableProperty]
    private string managedServiceImagePath = string.Empty;

    [ObservableProperty]
    private bool managedServiceHoliday;

    [ObservableProperty]
    private ThemeCardItem? selectedThemeOption;

    [ObservableProperty]
    private EmployeeChoice? selectedAssignmentMaster;

    [ObservableProperty]
    private EmployeeChoice? selectedBoundMaster;

    [ObservableProperty]
    private UserEditorItem? selectedManagedUser;

    [ObservableProperty]
    private string editedUserBalance = string.Empty;

    [ObservableProperty]
    private bool editedUserBlocked;

    [ObservableProperty]
    private string newEmployeeFullName = string.Empty;

    [ObservableProperty]
    private string newEmployeeLogin = string.Empty;

    [ObservableProperty]
    private string newEmployeeEmail = string.Empty;

    [ObservableProperty]
    private string newEmployeePhone = string.Empty;

    [ObservableProperty]
    private string newEmployeePassword = "demo123";

    [ObservableProperty]
    private string newEmployeeSpecialty = string.Empty;

    [ObservableProperty]
    private string newEmployeeAbout = string.Empty;

    [ObservableProperty]
    private UserRole newEmployeeRole = UserRole.Master;

    [ObservableProperty]
    private EmployeeChoice? selectedEmployeeForQualification;

    [ObservableProperty]
    private string qualificationRequestDesiredLevel = "4";

    [ObservableProperty]
    private string qualificationRequestComment = string.Empty;

    public bool IsAuthenticated => CurrentUser is not null;
    public bool IsGuestMode => !IsAuthenticated;
    public bool ShowLoginForm => !ShowRegistration;
    public bool IsClient => CurrentUser?.Role == UserRole.Client;
    public bool IsAdmin => CurrentUser?.Role == UserRole.Admin;
    public bool IsModerator => CurrentUser?.Role == UserRole.Moderator;
    public bool IsMaster => CurrentUser?.Role == UserRole.Master;
    public bool CanSeeAdminSection => IsAdmin;
    public bool CanSeeModeratorSection => IsAdmin || IsModerator;
    public bool CanSeeMasterSection => IsMaster;
    public bool IsHomeSection => CurrentSection == AppSection.Home;
    public bool IsCatalogSection => CurrentSection == AppSection.Catalog;
    public bool IsBookingsSection => CurrentSection == AppSection.Bookings;
    public bool IsBalanceSection => CurrentSection == AppSection.Balance;
    public bool IsReviewsSection => CurrentSection == AppSection.Reviews;
    public bool IsAdminSection => CurrentSection == AppSection.Admin;
    public bool IsModeratorSection => CurrentSection == AppSection.Moderator;
    public bool IsMasterSection => CurrentSection == AppSection.Master;
    public int TotalCatalogPages => Math.Max(1, (int)Math.Ceiling(_catalogSource.Count / (double)_options.PageSize));
    public string CurrentUserName => CurrentUser?.FullName ?? "Гость";
    public string CurrentRoleText => CurrentUser is null ? "не авторизован" : DisplayRole(CurrentUser.Role);
    public string CurrentBalanceText => CurrentUser is null ? "0 ₽" : $"{CurrentUser.Balance:N0} ₽";
    public string DatabaseModeText => IsUsingFallbackDatabase ? "Демо-режим (без сохранения в PostgreSQL)" : "PostgreSQL";
    public string AuthPanelTitle => ShowRegistration ? "Регистрация" : "Вход";
    public string AuthToggleText => ShowRegistration ? "У меня уже есть аккаунт" : "Создать новый аккаунт";
    public string HomeProductsTitle => SelectedHomeTheme is null ? "Товары коллекции" : $"Коллекция «{SelectedHomeTheme.Name}»";
    public string HomeProductsSubtitle => SelectedHomeTheme is null
        ? "Выберите направление слева."
        : $"{SelectedHomeTheme.Description} В подборке: {HomeThemeProducts.Count} услуг.";
    public string CatalogThemeText => SelectedCatalogTheme?.Name ?? "Все коллекции";
    public string CatalogSortText => SortCatalogAscending ? "Сортировка: A-Z" : "Сортировка: Z-A";
    public string CatalogPageText
    {
        get
        {
            if (_catalogSource.Count == 0)
            {
                return "0 из 0";
            }

            var start = ((CurrentCatalogPage - 1) * _options.PageSize) + 1;
            var end = Math.Min(CurrentCatalogPage * _options.PageSize, _catalogSource.Count);
            return $"{start}-{end} из {_catalogSource.Count}";
        }
    }

    partial void OnCurrentUserChanged(User? value)
    {
        if (value is not null)
        {
            CurrentSection = value.Role switch
            {
                UserRole.Admin => AppSection.Admin,
                UserRole.Moderator => AppSection.Moderator,
                UserRole.Master => AppSection.Master,
                _ => AppSection.Catalog
            };
        }
        else
        {
            CurrentSection = AppSection.Home;
        }

        RaiseShellState();
        ReloadData();
    }

    partial void OnShowRegistrationChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowLoginForm));
        OnPropertyChanged(nameof(AuthPanelTitle));
        OnPropertyChanged(nameof(AuthToggleText));
    }

    partial void OnSelectedHomeThemeChanged(ThemeCardItem? value)
    {
        LoadHomeThemeProducts();
        OnPropertyChanged(nameof(HomeProductsTitle));
        OnPropertyChanged(nameof(HomeProductsSubtitle));
    }

    partial void OnSelectedCatalogThemeChanged(ThemeCardItem? value)
    {
        CurrentCatalogPage = 1;
        BuildCatalogSource();
        RefreshCatalogPage();
        OnPropertyChanged(nameof(CatalogThemeText));
    }

    partial void OnCatalogSearchTextChanged(string value)
    {
        CurrentCatalogPage = 1;
        BuildCatalogSource();
        RefreshCatalogPage();
    }

    partial void OnCurrentSectionChanged(AppSection value)
    {
        OnPropertyChanged(nameof(IsHomeSection));
        OnPropertyChanged(nameof(IsCatalogSection));
        OnPropertyChanged(nameof(IsBookingsSection));
        OnPropertyChanged(nameof(IsBalanceSection));
        OnPropertyChanged(nameof(IsReviewsSection));
        OnPropertyChanged(nameof(IsAdminSection));
        OnPropertyChanged(nameof(IsModeratorSection));
        OnPropertyChanged(nameof(IsMasterSection));
    }

    partial void OnSelectedCatalogItemChanged(ServiceCatalogItem? value)
    {
        SelectedCatalogMaster = value?.Masters.FirstOrDefault();
    }

    partial void OnSelectedManagedServiceChanged(ServiceEditorItem? value)
    {
        if (_suspendServiceSelectionUpdate)
        {
            return;
        }

        if (value is null)
        {
            ResetManagedServiceForm();
            return;
        }

        var service = _db.ServiceItems
            .Include(x => x.CollectionTheme)
            .Include(x => x.ServiceAssignments)
            .ThenInclude(x => x.Employee)
            .ThenInclude(x => x.User)
            .First(x => x.Id == value.Id);

        ManagedServiceName = service.Name;
        ManagedServiceDescription = service.Description;
        ManagedServicePrice = service.Price.ToString("0.##");
        ManagedServiceDuration = service.DurationMinutes.ToString();
        ManagedServiceImagePath = NormalizeAssetPath(service.ImagePath);
        ManagedServiceHoliday = service.IsHoliday;
        SelectedThemeOption = ThemeOptions.FirstOrDefault(x => x.Id == service.CollectionThemeId);

        ReplaceCollection(
            BoundMasters,
            service.ServiceAssignments
                .OrderBy(x => x.Employee.User.FullName)
                .Select(x => new EmployeeChoice
                {
                    Id = x.EmployeeId,
                    DisplayName = x.Employee.User.FullName,
                    QualificationLevel = x.Employee.QualificationLevel
                }));
    }

    partial void OnSelectedManagedUserChanged(UserEditorItem? value)
    {
        if (value is null)
        {
            EditedUserBalance = "0";
            EditedUserBlocked = false;
            return;
        }

        EditedUserBalance = value.Balance.ToString("0.##");
        EditedUserBlocked = value.IsBlocked;
    }

    [RelayCommand]
    private void OpenHome() => CurrentSection = AppSection.Home;

    [RelayCommand]
    private void SelectCatalogTheme(ThemeCardItem? theme)
    {
        SelectedCatalogTheme = theme;
        CurrentSection = AppSection.Catalog;
    }

    [RelayCommand]
    private void ClearCatalogTheme()
    {
        SelectedCatalogTheme = null;
        CurrentSection = AppSection.Catalog;
    }

    [RelayCommand]
    private void OpenCatalog() => CurrentSection = AppSection.Catalog;

    [RelayCommand]
    private void OpenBookings() => CurrentSection = AppSection.Bookings;

    [RelayCommand]
    private void OpenBalance() => CurrentSection = AppSection.Balance;

    [RelayCommand]
    private void OpenReviews() => CurrentSection = AppSection.Reviews;

    [RelayCommand]
    private void OpenAdmin()
    {
        if (CanSeeAdminSection)
        {
            CurrentSection = AppSection.Admin;
        }
    }

    [RelayCommand]
    private void OpenModerator()
    {
        if (CanSeeModeratorSection)
        {
            CurrentSection = AppSection.Moderator;
        }
    }

    [RelayCommand]
    private void OpenMaster()
    {
        if (CanSeeMasterSection)
        {
            CurrentSection = AppSection.Master;
        }
    }

    [RelayCommand]
    private void ToggleRegistration()
    {
        ShowRegistration = !ShowRegistration;
        StatusMessage = ShowRegistration
            ? "Создайте клиентский аккаунт для онлайн-записи."
            : "Введите логин и пароль. Демо-пользователь: client / demo123.";
    }

    [RelayCommand]
    private void LoginUser()
    {
        var normalizedLogin = Login.Trim();
        var user = _db.Users.FirstOrDefault(x => x.Login == normalizedLogin);
        if (user is null || !PasswordHelper.Verify(Password.Trim(), user.PasswordHash))
        {
            StatusMessage = "Неверный логин или пароль.";
            return;
        }

        if (user.IsBlocked)
        {
            StatusMessage = "Пользователь заблокирован администратором.";
            return;
        }

        CurrentUser = user;
        ShowRegistration = false;
        StatusMessage = $"Вход выполнен: {user.FullName}.";
    }

    [RelayCommand]
    private void RegisterUser()
    {
        var fullName = RegisterFullName.Trim();
        var loginValue = RegisterLogin.Trim();
        var email = RegisterEmail.Trim();

        if (string.IsNullOrWhiteSpace(fullName)
            || string.IsNullOrWhiteSpace(loginValue)
            || string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(RegisterPassword))
        {
            StatusMessage = "Заполните ФИО, логин, email и пароль.";
            return;
        }

        if (_db.Users.Any(x => x.Login == loginValue))
        {
            StatusMessage = "Такой логин уже существует.";
            return;
        }

        if (_db.Users.Any(x => x.Email == email))
        {
            StatusMessage = "Такой email уже используется.";
            return;
        }

        var user = new User
        {
            FullName = fullName,
            Login = loginValue,
            Email = email,
            Phone = RegisterPhone.Trim(),
            PasswordHash = PasswordHelper.Hash(RegisterPassword.Trim()),
            Role = UserRole.Client,
            Balance = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db.Users.Add(user);
        _db.SaveChanges();

        CurrentUser = user;
        RegisterFullName = string.Empty;
        RegisterLogin = string.Empty;
        RegisterEmail = string.Empty;
        RegisterPhone = string.Empty;
        RegisterPassword = string.Empty;
        StatusMessage = WithPersistenceNote("Регистрация завершена. Можно сразу оформлять запись на услугу.");
    }

    [RelayCommand]
    private void Logout()
    {
        CurrentUser = null;
        Login = "client";
        Password = "demo123";
        ShowRegistration = false;
        StatusMessage = "Вы вышли из аккаунта.";
    }

    [RelayCommand]
    private void PreviousCatalogPage()
    {
        if (CurrentCatalogPage > 1)
        {
            CurrentCatalogPage--;
            RefreshCatalogPage();
        }
    }

    [RelayCommand]
    private void NextCatalogPage()
    {
        if (CurrentCatalogPage < TotalCatalogPages)
        {
            CurrentCatalogPage++;
            RefreshCatalogPage();
        }
    }

    [RelayCommand]
    private void ToggleCatalogSort()
    {
        SortCatalogAscending = !SortCatalogAscending;
        CurrentCatalogPage = 1;
        BuildCatalogSource();
        RefreshCatalogPage();
        OnPropertyChanged(nameof(CatalogSortText));
    }

    [RelayCommand]
    private void BookSelectedService()
    {
        if (!IsClient || CurrentUser is null)
        {
            StatusMessage = "Записывать на услуги может только клиент.";
            return;
        }

        if (SelectedCatalogItem is null || SelectedCatalogMaster is null || SelectedBookingSlot is null)
        {
            StatusMessage = "Выберите услугу, мастера и слот времени.";
            return;
        }

        var service = _db.ServiceItems.First(x => x.Id == SelectedCatalogItem.Id);
        var user = _db.Users.First(x => x.Id == CurrentUser.Id);
        if (user.Balance < service.Price)
        {
            StatusMessage = "Недостаточно средств на балансе. Пополните счёт перед записью.";
            return;
        }

        var queueNumber = _db.Bookings.Count(x => x.EmployeeId == SelectedCatalogMaster.Id && x.Status == BookingStatus.Planned) + 1;
        var booking = new Booking
        {
            CustomerId = user.Id,
            EmployeeId = SelectedCatalogMaster.Id,
            ServiceItemId = service.Id,
            ScheduledAt = SelectedBookingSlot.Value,
            QueueNumber = queueNumber,
            Status = BookingStatus.Planned,
            Notes = BookingNotes.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        user.Balance -= service.Price;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        _db.Bookings.Add(booking);
        _db.SaveChanges();

        RaiseShellState();
        BookingNotes = string.Empty;
        StatusMessage = WithPersistenceNote($"Запись оформлена. Ваш номер в очереди: {queueNumber}.");
        ReloadData();
        CurrentSection = AppSection.Bookings;
    }

    [RelayCommand]
    private void TopUpBalance()
    {
        if (CurrentUser is null)
        {
            StatusMessage = "Сначала войдите в аккаунт.";
            return;
        }

        if (!decimal.TryParse(TopUpAmount, out var amount) || amount <= 0)
        {
            StatusMessage = "Введите корректную сумму пополнения.";
            return;
        }

        var user = _db.Users.First(x => x.Id == CurrentUser.Id);
        user.Balance += amount;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        _db.TopUpTransactions.Add(new TopUpTransaction
        {
            UserId = user.Id,
            Amount = amount,
            CardMask = MaskCardNumber(TopUpCardNumber),
            CreatedAt = DateTimeOffset.UtcNow
        });
        _db.SaveChanges();

        RaiseShellState();
        StatusMessage = WithPersistenceNote($"Баланс пополнен на {amount:N0} ₽.");
        ReloadData();
    }

    [RelayCommand]
    private void SubmitReview()
    {
        if (CurrentUser is null || SelectedBookingForReview is null)
        {
            StatusMessage = "Выберите завершённую запись для отзыва.";
            return;
        }

        var booking = _db.Bookings.FirstOrDefault(x => x.Id == SelectedBookingForReview.Id);
        if (booking is null)
        {
            StatusMessage = "Запись для отзыва не найдена.";
            return;
        }

        _db.Reviews.Add(new Review
        {
            CustomerId = CurrentUser.Id,
            BookingId = booking.Id,
            EmployeeId = booking.EmployeeId,
            ServiceItemId = booking.ServiceItemId,
            Rating = SelectedRating,
            Comment = ReviewComment.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        });
        _db.SaveChanges();

        ReviewComment = string.Empty;
        SelectedBookingForReview = null;
        StatusMessage = WithPersistenceNote("Отзыв сохранён.");
        ReloadData();
    }

    [RelayCommand]
    private void CreateNewServiceDraft()
    {
        _suspendServiceSelectionUpdate = true;
        SelectedManagedService = null;
        _suspendServiceSelectionUpdate = false;
        ResetManagedServiceForm();
        StatusMessage = "Форма новой услуги очищена.";
    }

    [RelayCommand]
    private void SaveManagedService()
    {
        if (!CanSeeModeratorSection)
        {
            StatusMessage = "Недостаточно прав для редактирования услуг.";
            return;
        }

        if (!decimal.TryParse(ManagedServicePrice, out var price) || price <= 0
            || !int.TryParse(ManagedServiceDuration, out var duration) || duration <= 0
            || string.IsNullOrWhiteSpace(ManagedServiceName)
            || SelectedThemeOption is null)
        {
            StatusMessage = "Проверьте название, цену, длительность и выбранную коллекцию.";
            return;
        }

        var successMessage = string.Empty;
        ServiceItem service;
        if (SelectedManagedService is null)
        {
            service = new ServiceItem
            {
                Name = ManagedServiceName.Trim(),
                Description = ManagedServiceDescription.Trim(),
                Price = price,
                DurationMinutes = duration,
                ImagePath = NormalizeAssetPath(string.IsNullOrWhiteSpace(ManagedServiceImagePath) ? "/Assets/Images/Кастом/Pr3.jpg" : ManagedServiceImagePath.Trim()),
                IsHoliday = ManagedServiceHoliday,
                CollectionThemeId = SelectedThemeOption.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                LastModifiedAt = LastModifiedService.Touch()
            };
            _db.ServiceItems.Add(service);
            successMessage = "Новая услуга добавлена.";
        }
        else
        {
            service = _db.ServiceItems.First(x => x.Id == SelectedManagedService.Id);
            service.Name = ManagedServiceName.Trim();
            service.Description = ManagedServiceDescription.Trim();
            service.Price = price;
            service.DurationMinutes = duration;
            service.ImagePath = string.IsNullOrWhiteSpace(ManagedServiceImagePath)
                ? NormalizeAssetPath(service.ImagePath)
                : NormalizeAssetPath(ManagedServiceImagePath.Trim());
            service.IsHoliday = ManagedServiceHoliday;
            service.CollectionThemeId = SelectedThemeOption.Id;
            service.LastModifiedAt = LastModifiedService.Touch();
            successMessage = "Изменения по услуге сохранены.";
        }

        _db.SaveChanges();
        StatusMessage = WithPersistenceNote(successMessage);
        ReloadData();
    }

    [RelayCommand]
    private void BindMasterToService()
    {
        if (SelectedManagedService is null || SelectedAssignmentMaster is null)
        {
            StatusMessage = "Выберите услугу и мастера для привязки.";
            return;
        }

        var alreadyExists = _db.ServiceAssignments.Any(x =>
            x.ServiceItemId == SelectedManagedService.Id &&
            x.EmployeeId == SelectedAssignmentMaster.Id);

        if (alreadyExists)
        {
            StatusMessage = "Этот мастер уже привязан к услуге.";
            return;
        }

        _db.ServiceAssignments.Add(new ServiceAssignment
        {
            EmployeeId = SelectedAssignmentMaster.Id,
            ServiceItemId = SelectedManagedService.Id,
            AttachedAt = DateTimeOffset.UtcNow
        });
        _db.SaveChanges();

        StatusMessage = WithPersistenceNote("Мастер привязан к услуге.");
        ReloadData();
        SelectedManagedService = ManagedServices.FirstOrDefault(x => x.Id == SelectedManagedService.Id);
    }

    [RelayCommand]
    private void UnbindMasterFromService()
    {
        if (SelectedManagedService is null || SelectedBoundMaster is null)
        {
            StatusMessage = "Выберите мастера из списка привязанных.";
            return;
        }

        var assignment = _db.ServiceAssignments.FirstOrDefault(x =>
            x.ServiceItemId == SelectedManagedService.Id &&
            x.EmployeeId == SelectedBoundMaster.Id);
        if (assignment is null)
        {
            StatusMessage = "Связь не найдена.";
            return;
        }

        _db.ServiceAssignments.Remove(assignment);
        _db.SaveChanges();

        StatusMessage = WithPersistenceNote("Привязка удалена.");
        ReloadData();
        SelectedManagedService = ManagedServices.FirstOrDefault(x => x.Id == SelectedManagedService.Id);
    }

    [RelayCommand]
    private void SaveSelectedUser()
    {
        if (!IsAdmin || SelectedManagedUser is null)
        {
            StatusMessage = "Пользователь для редактирования не выбран.";
            return;
        }

        if (!decimal.TryParse(EditedUserBalance, out var balance) || balance < 0)
        {
            StatusMessage = "Введите корректный баланс.";
            return;
        }

        var user = _db.Users.First(x => x.Id == SelectedManagedUser.Id);
        user.Balance = balance;
        user.IsBlocked = EditedUserBlocked;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        _db.SaveChanges();

        RaiseShellState();
        StatusMessage = WithPersistenceNote("Профиль пользователя обновлён.");
        ReloadData();
    }

    [RelayCommand]
    private void CreateEmployee()
    {
        if (!IsAdmin)
        {
            StatusMessage = "Добавлять сотрудников может только администратор.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewEmployeeFullName)
            || string.IsNullOrWhiteSpace(NewEmployeeLogin)
            || string.IsNullOrWhiteSpace(NewEmployeeEmail))
        {
            StatusMessage = "Заполните имя, логин и email нового сотрудника.";
            return;
        }

        if (_db.Users.Any(x => x.Login == NewEmployeeLogin.Trim()))
        {
            StatusMessage = "Логин сотрудника уже занят.";
            return;
        }

        var user = new User
        {
            FullName = NewEmployeeFullName.Trim(),
            Login = NewEmployeeLogin.Trim(),
            Email = NewEmployeeEmail.Trim(),
            Phone = NewEmployeePhone.Trim(),
            PasswordHash = PasswordHelper.Hash(NewEmployeePassword.Trim()),
            Role = NewEmployeeRole,
            Balance = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _db.Users.Add(user);

        if (NewEmployeeRole is UserRole.Master or UserRole.Moderator)
        {
            _db.Employees.Add(new Employee
            {
                User = user,
                Specialty = string.IsNullOrWhiteSpace(NewEmployeeSpecialty) ? "Новый сотрудник" : NewEmployeeSpecialty.Trim(),
                QualificationLevel = 1,
                About = NewEmployeeAbout.Trim(),
                HireDate = DateTimeOffset.UtcNow
            });
        }

        _db.SaveChanges();
        ResetEmployeeForm();
        StatusMessage = WithPersistenceNote("Сотрудник успешно добавлен.");
        ReloadData();
    }

    [RelayCommand]
    private void RaiseEmployeeQualification()
    {
        if (!CanSeeModeratorSection || SelectedEmployeeForQualification is null)
        {
            StatusMessage = "Выберите мастера для повышения квалификации.";
            return;
        }

        var employee = _db.Employees.Include(x => x.User).First(x => x.Id == SelectedEmployeeForQualification.Id);
        employee.QualificationLevel += 1;
        employee.User.UpdatedAt = DateTimeOffset.UtcNow;

        var pendingRequest = _db.QualificationRequests
            .Where(x => x.EmployeeId == employee.Id && x.Status == QualificationRequestStatus.Pending)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault();

        if (pendingRequest is not null)
        {
            pendingRequest.Status = QualificationRequestStatus.Approved;
        }

        _db.SaveChanges();
        StatusMessage = WithPersistenceNote($"Квалификация сотрудника {employee.User.FullName} повышена до уровня {employee.QualificationLevel}.");
        ReloadData();
    }

    [RelayCommand]
    private void SubmitQualificationRequest()
    {
        if (!IsMaster || CurrentUser is null)
        {
            StatusMessage = "Эта функция доступна только мастеру.";
            return;
        }

        if (!int.TryParse(QualificationRequestDesiredLevel, out var desiredLevel) || desiredLevel < 1)
        {
            StatusMessage = "Укажите целевой уровень квалификации.";
            return;
        }

        var employee = _db.Employees.FirstOrDefault(x => x.UserId == CurrentUser.Id);
        if (employee is null)
        {
            StatusMessage = "Профиль сотрудника не найден.";
            return;
        }

        _db.QualificationRequests.Add(new QualificationRequest
        {
            EmployeeId = employee.Id,
            DesiredLevel = desiredLevel,
            Comment = QualificationRequestComment.Trim(),
            Status = QualificationRequestStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        });
        _db.SaveChanges();

        StatusMessage = WithPersistenceNote("Заявка на повышение квалификации отправлена.");
        ReloadData();
    }

    public void RequestClose()
    {
        ShowExitDialog = true;
    }

    private string WithPersistenceNote(string message)
    {
        return IsUsingFallbackDatabase
            ? $"{message} Сейчас приложение работает в демо-режиме: изменения не сохраняются в PostgreSQL."
            : message;
    }

    private void ReloadData()
    {
        BuildCatalogSource();
        LoadHomeThemeProducts();
        RefreshCatalogPage();
        LoadClientBookings();
        LoadReviewFeed();
        LoadManagementCollections();
        LoadManagementServices();
        LoadUserDirectory();
        LoadEmployeeDirectory();
        LoadMasterBookings();
        LoadQualificationRequests();
        OnPropertyChanged(nameof(CurrentUserName));
        OnPropertyChanged(nameof(CurrentRoleText));
        OnPropertyChanged(nameof(CurrentBalanceText));
        OnPropertyChanged(nameof(DatabaseModeText));
        OnPropertyChanged(nameof(CatalogThemeText));
        OnPropertyChanged(nameof(CatalogSortText));
        OnPropertyChanged(nameof(CatalogPageText));
    }

    private void LoadThemeCards()
    {
        var selectedThemeId = SelectedHomeTheme?.Id;
        var selectedCatalogThemeId = SelectedCatalogTheme?.Id;
        ReplaceCollection(
            ThemeCards,
            _db.CollectionThemes
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new ThemeCardItem
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    IsHoliday = x.IsHoliday,
                    ImagePath = NormalizeAssetPath(x.ImagePath)
                })
                .ToList());

        SelectedHomeTheme = ThemeCards.FirstOrDefault(x => x.Id == selectedThemeId) ?? ThemeCards.FirstOrDefault();
        SelectedCatalogTheme = ThemeCards.FirstOrDefault(x => x.Id == selectedCatalogThemeId);
    }

    private void BuildCatalogSource()
    {
        var searchTerm = CatalogSearchText.Trim();
        _catalogSource = _db.ServiceItems
            .AsNoTracking()
            .Include(x => x.CollectionTheme)
            .Include(x => x.ServiceAssignments)
            .ThenInclude(x => x.Employee)
            .ThenInclude(x => x.User)
            .ToList()
            .Where(x => SelectedCatalogTheme is null || x.CollectionThemeId == SelectedCatalogTheme.Id)
            .Where(x =>
                string.IsNullOrWhiteSpace(searchTerm)
                || ContainsIgnoreCase(x.Name, searchTerm)
                || ContainsIgnoreCase(x.Description, searchTerm)
                || ContainsIgnoreCase(x.CollectionTheme.Name, searchTerm))
            .OrderByDirection(SortCatalogAscending, x => x.Name)
            .Select(x => new ServiceCatalogItem
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ThemeName = x.CollectionTheme.Name,
                Price = x.Price,
                DurationMinutes = x.DurationMinutes,
                ImagePath = NormalizeAssetPath(x.ImagePath),
                IsHoliday = x.IsHoliday,
                LastModifiedAt = x.LastModifiedAt.ToLocalTime(),
                LastModifiedText = LastModifiedService.Describe(x.LastModifiedAt),
                Masters = x.ServiceAssignments
                    .OrderBy(y => y.Employee.User.FullName)
                    .Select(y => new EmployeeChoice
                    {
                        Id = y.EmployeeId,
                        DisplayName = y.Employee.User.FullName,
                        QualificationLevel = y.Employee.QualificationLevel
                    })
                    .ToList()
            })
            .ToList();

        CurrentCatalogPage = Math.Min(CurrentCatalogPage, TotalCatalogPages);
    }

    private void LoadHomeThemeProducts()
    {
        var products = SelectedHomeTheme is null
            ? []
            : _catalogSource
                .Where(x => x.ThemeName == SelectedHomeTheme.Name)
                .OrderBy(x => x.Name)
                .Take(6)
                .ToList();

        ReplaceCollection(HomeThemeProducts, products);
        OnPropertyChanged(nameof(HomeProductsTitle));
        OnPropertyChanged(nameof(HomeProductsSubtitle));
    }

    private void RefreshCatalogPage()
    {
        if (_catalogSource.Count == 0)
        {
            ReplaceCollection(CatalogItems, []);
            SelectedCatalogItem = null;
            OnPropertyChanged(nameof(TotalCatalogPages));
            return;
        }

        var pageItems = _catalogSource
            .Skip((CurrentCatalogPage - 1) * _options.PageSize)
            .Take(_options.PageSize)
            .ToList();

        ReplaceCollection(CatalogItems, pageItems);
        SelectedCatalogItem = CatalogItems.FirstOrDefault();
        if (SelectedBookingSlot is null)
        {
            SelectedBookingSlot = BookingSlots.FirstOrDefault();
        }

        OnPropertyChanged(nameof(TotalCatalogPages));
        OnPropertyChanged(nameof(CatalogPageText));
    }

    private void LoadClientBookings()
    {
        if (!IsClient || CurrentUser is null)
        {
            ReplaceCollection(ClientBookings, []);
            return;
        }

        var bookings = _db.Bookings
            .AsNoTracking()
            .Include(x => x.Employee)
            .ThenInclude(x => x.User)
            .Include(x => x.ServiceItem)
            .Where(x => x.CustomerId == CurrentUser.Id)
            .OrderByDescending(x => x.CreatedAt)
            .AsEnumerable()
            .Select(x => new BookingItem
            {
                Id = x.Id,
                ServiceId = x.ServiceItemId,
                EmployeeId = x.EmployeeId,
                ServiceName = x.ServiceItem.Name,
                MasterName = x.Employee.User.FullName,
                ScheduledAt = x.ScheduledAt.ToLocalTime(),
                QueueNumber = x.QueueNumber,
                StatusText = DisplayBookingStatus(x.Status),
                ImagePath = NormalizeAssetPath(x.ServiceItem.ImagePath),
                CanReview = x.Status == BookingStatus.Completed
            })
            .ToList();

        ReplaceCollection(ClientBookings, bookings);
    }

    private void LoadReviewFeed()
    {
        var reviews = _db.Reviews
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Employee)
            .ThenInclude(x => x!.User)
            .Include(x => x.ServiceItem)
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .AsEnumerable()
            .Select(x => new ReviewItem
            {
                CustomerName = x.Customer.FullName,
                ServiceName = x.ServiceItem != null ? x.ServiceItem.Name : "услуга не указана",
                MasterName = x.Employee != null ? x.Employee.User.FullName : "мастер не указан",
                Rating = x.Rating,
                Comment = x.Comment,
                CreatedAt = x.CreatedAt.ToLocalTime()
            })
            .ToList();

        ReplaceCollection(ReviewFeed, reviews);
    }

    private void LoadManagementCollections()
    {
        ReplaceCollection(ThemeOptions, ThemeCards);
        SelectedThemeOption ??= ThemeOptions.FirstOrDefault();
    }

    private void LoadManagementServices()
    {
        if (!CanSeeModeratorSection)
        {
            ReplaceCollection(ManagedServices, []);
            ReplaceCollection(BoundMasters, []);
            return;
        }

        var services = _db.ServiceItems
            .AsNoTracking()
            .Include(x => x.CollectionTheme)
            .OrderBy(x => x.Name)
            .AsEnumerable()
            .Select(x => new ServiceEditorItem
            {
                Id = x.Id,
                Name = x.Name,
                ThemeName = x.CollectionTheme.Name,
                Price = x.Price,
                ImagePath = NormalizeAssetPath(x.ImagePath),
                LastModifiedAt = x.LastModifiedAt.ToLocalTime()
            })
            .ToList();
        ReplaceCollection(ManagedServices, services);

        var masters = _db.Employees
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.User.Role == UserRole.Master)
            .OrderBy(x => x.User.FullName)
            .Select(x => new EmployeeChoice
            {
                Id = x.Id,
                DisplayName = x.User.FullName,
                QualificationLevel = x.QualificationLevel
            })
            .ToList();

        ReplaceCollection(AvailableMasters, masters);
        ReplaceCollection(EmployeeDirectory, masters);
        SelectedAssignmentMaster ??= AvailableMasters.FirstOrDefault();
        SelectedEmployeeForQualification ??= EmployeeDirectory.FirstOrDefault();
    }

    private void LoadUserDirectory()
    {
        if (!IsAdmin)
        {
            ReplaceCollection(ManagedUsers, []);
            return;
        }

        var users = _db.Users
            .AsNoTracking()
            .OrderBy(x => x.FullName)
            .Select(x => new UserEditorItem
            {
                Id = x.Id,
                FullName = x.FullName,
                Login = x.Login,
                Email = x.Email,
                Role = x.Role,
                Balance = x.Balance,
                IsBlocked = x.IsBlocked
            })
            .ToList();
        ReplaceCollection(ManagedUsers, users);
        SelectedManagedUser ??= ManagedUsers.FirstOrDefault();
    }

    private void LoadEmployeeDirectory()
    {
        if (!CanSeeModeratorSection)
        {
            ReplaceCollection(EmployeeDirectory, []);
            return;
        }

        var employees = _db.Employees
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.User.Role == UserRole.Master)
            .OrderBy(x => x.User.FullName)
            .Select(x => new EmployeeChoice
            {
                Id = x.Id,
                DisplayName = x.User.FullName,
                QualificationLevel = x.QualificationLevel
            })
            .ToList();

        ReplaceCollection(EmployeeDirectory, employees);
        SelectedEmployeeForQualification ??= EmployeeDirectory.FirstOrDefault();
    }

    private void LoadMasterBookings()
    {
        if (!IsMaster || CurrentUser is null)
        {
            ReplaceCollection(MasterBookings, []);
            return;
        }

        var employeeId = _db.Employees
            .AsNoTracking()
            .Where(x => x.UserId == CurrentUser.Id)
            .Select(x => x.Id)
            .FirstOrDefault();

        if (employeeId == 0)
        {
            ReplaceCollection(MasterBookings, []);
            return;
        }

        var bookings = _db.Bookings
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.ServiceItem)
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.ScheduledAt)
            .AsEnumerable()
            .Select(x => new BookingItem
            {
                Id = x.Id,
                ServiceId = x.ServiceItemId,
                EmployeeId = x.EmployeeId,
                ServiceName = $"{x.ServiceItem.Name} для {x.Customer.FullName}",
                MasterName = x.Customer.Phone,
                ScheduledAt = x.ScheduledAt.ToLocalTime(),
                QueueNumber = x.QueueNumber,
                StatusText = DisplayBookingStatus(x.Status),
                ImagePath = NormalizeAssetPath(x.ServiceItem.ImagePath),
                CanReview = false
            })
            .ToList();

        ReplaceCollection(MasterBookings, bookings);
    }

    private void LoadQualificationRequests()
    {
        if (!CanSeeModeratorSection && !IsMaster)
        {
            ReplaceCollection(QualificationRequests, []);
            return;
        }

        var requests = _db.QualificationRequests
            .AsNoTracking()
            .Include(x => x.Employee)
            .ThenInclude(x => x.User)
            .OrderByDescending(x => x.CreatedAt)
            .AsEnumerable()
            .Select(x => new QualificationRequestItem
            {
                Id = x.Id,
                EmployeeName = x.Employee.User.FullName,
                DesiredLevel = x.DesiredLevel,
                Comment = x.Comment,
                StatusText = DisplayQualificationStatus(x.Status),
                CreatedAt = x.CreatedAt.ToLocalTime()
            })
            .ToList();

        ReplaceCollection(QualificationRequests, requests);
    }

    private void BuildBookingSlots()
    {
        var localNow = DateTimeOffset.Now;
        var start = localNow.AddDays(1).Date;
        var offset = localNow.Offset;
        var slots = new List<BookingSlotOption>();
        for (var day = 0; day < 7; day++)
        {
            foreach (var hour in new[] { 12, 15, 18 })
            {
                var localSlot = new DateTimeOffset(start.Year, start.Month, start.Day, 0, 0, 0, offset)
                    .AddDays(day)
                    .AddHours(hour);
                slots.Add(new BookingSlotOption
                {
                    Label = localSlot.ToString("dd.MM HH:mm"),
                    Value = localSlot.ToUniversalTime()
                });
            }
        }

        ReplaceCollection(BookingSlots, slots);
        SelectedBookingSlot = BookingSlots.FirstOrDefault();
    }

    private void ResetManagedServiceForm()
    {
        ManagedServiceName = string.Empty;
        ManagedServiceDescription = string.Empty;
        ManagedServicePrice = "3200";
        ManagedServiceDuration = "90";
        ManagedServiceImagePath = "/Assets/Images/Кастом/Pr3.jpg";
        ManagedServiceHoliday = false;
        SelectedThemeOption = ThemeOptions.FirstOrDefault();
        ReplaceCollection(BoundMasters, []);
    }

    private void ResetEmployeeForm()
    {
        NewEmployeeFullName = string.Empty;
        NewEmployeeLogin = string.Empty;
        NewEmployeeEmail = string.Empty;
        NewEmployeePhone = string.Empty;
        NewEmployeePassword = "demo123";
        NewEmployeeSpecialty = string.Empty;
        NewEmployeeAbout = string.Empty;
        NewEmployeeRole = UserRole.Master;
    }

    private void RaiseShellState()
    {
        OnPropertyChanged(nameof(IsAuthenticated));
        OnPropertyChanged(nameof(IsGuestMode));
        OnPropertyChanged(nameof(IsClient));
        OnPropertyChanged(nameof(IsAdmin));
        OnPropertyChanged(nameof(IsModerator));
        OnPropertyChanged(nameof(IsMaster));
        OnPropertyChanged(nameof(CanSeeAdminSection));
        OnPropertyChanged(nameof(CanSeeModeratorSection));
        OnPropertyChanged(nameof(CanSeeMasterSection));
        OnPropertyChanged(nameof(CurrentUserName));
        OnPropertyChanged(nameof(CurrentRoleText));
        OnPropertyChanged(nameof(CurrentBalanceText));
    }

    private static void ReplaceCollection<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();
        foreach (var item in items)
        {
            target.Add(item);
        }
    }

    private static bool ContainsIgnoreCase(string? source, string value)
    {
        return !string.IsNullOrWhiteSpace(source)
            && source.Contains(value, StringComparison.OrdinalIgnoreCase);
    }

    private static string DisplayRole(UserRole role)
    {
        return role switch
        {
            UserRole.Admin => "администратор",
            UserRole.Moderator => "модератор",
            UserRole.Master => "мастер",
            _ => "клиент"
        };
    }

    private static string DisplayBookingStatus(BookingStatus status)
    {
        return status switch
        {
            BookingStatus.Completed => "завершена",
            BookingStatus.Cancelled => "отменена",
            _ => "запланирована"
        };
    }

    private static string DisplayQualificationStatus(QualificationRequestStatus status)
    {
        return status switch
        {
            QualificationRequestStatus.Approved => "одобрена",
            QualificationRequestStatus.Rejected => "отклонена",
            _ => "на рассмотрении"
        };
    }

    private static string MaskCardNumber(string value)
    {
        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.Length < 4)
        {
            return "карта не указана";
        }

        return $"2200 **** **** {digits[^4..]}";
    }

    private static string NormalizeAssetPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/Assets/Logo.png";
        }

        return path
            .Replace('\\', '/')
            .Replace("РљР°СЃС‚РѕРј", "Кастом")
            .Replace("РљРѕСЃРїР»РµР№", "Косплей");
    }
}

file static class EnumerableExtensions
{
    public static IEnumerable<TSource> OrderByDirection<TSource, TKey>(this IEnumerable<TSource> source, bool ascending, Func<TSource, TKey> keySelector)
    {
        return ascending ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
    }
}
