namespace MathieuShop.Avalonia.Data;

public enum UserRole
{
    Client = 0,
    Moderator = 1,
    Admin = 2,
    Master = 3
}

public enum BookingStatus
{
    Planned = 0,
    Completed = 1,
    Cancelled = 2
}

public enum QualificationRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public sealed class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public decimal Balance { get; set; }
    public bool IsBlocked { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Employee? EmployeeProfile { get; set; }
    public List<Booking> CustomerBookings { get; set; } = [];
    public List<Review> Reviews { get; set; } = [];
    public List<TopUpTransaction> TopUps { get; set; } = [];
}

public sealed class Employee
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Specialty { get; set; } = string.Empty;
    public int QualificationLevel { get; set; }
    public string About { get; set; } = string.Empty;
    public DateTimeOffset HireDate { get; set; }

    public User User { get; set; } = null!;
    public List<ServiceAssignment> ServiceAssignments { get; set; } = [];
    public List<Booking> AssignedBookings { get; set; } = [];
    public List<QualificationRequest> QualificationRequests { get; set; } = [];
}

public sealed class CollectionTheme
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsHoliday { get; set; }
    public string ImagePath { get; set; } = string.Empty;

    public List<ServiceItem> Services { get; set; } = [];
}

public sealed class ServiceItem
{
    public int Id { get; set; }
    public int CollectionThemeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public bool IsHoliday { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastModifiedAt { get; set; }

    public CollectionTheme CollectionTheme { get; set; } = null!;
    public List<ServiceAssignment> ServiceAssignments { get; set; } = [];
    public List<Booking> Bookings { get; set; } = [];
    public List<Review> Reviews { get; set; } = [];
}

public sealed class ServiceAssignment
{
    public int EmployeeId { get; set; }
    public int ServiceItemId { get; set; }
    public DateTimeOffset AttachedAt { get; set; }

    public Employee Employee { get; set; } = null!;
    public ServiceItem ServiceItem { get; set; } = null!;
}

public sealed class Booking
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int EmployeeId { get; set; }
    public int ServiceItemId { get; set; }
    public DateTimeOffset ScheduledAt { get; set; }
    public int QueueNumber { get; set; }
    public BookingStatus Status { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public User Customer { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
    public ServiceItem ServiceItem { get; set; } = null!;
    public List<Review> Reviews { get; set; } = [];
}

public sealed class Review
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int? BookingId { get; set; }
    public int? EmployeeId { get; set; }
    public int? ServiceItemId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public User Customer { get; set; } = null!;
    public Booking? Booking { get; set; }
    public Employee? Employee { get; set; }
    public ServiceItem? ServiceItem { get; set; }
}

public sealed class TopUpTransaction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public string CardMask { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public User User { get; set; } = null!;
}

public sealed class QualificationRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int DesiredLevel { get; set; }
    public string Comment { get; set; } = string.Empty;
    public QualificationRequestStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Employee Employee { get; set; } = null!;
}
