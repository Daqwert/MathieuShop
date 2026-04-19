using Microsoft.EntityFrameworkCore;

namespace MathieuShop.Avalonia.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<CollectionTheme> CollectionThemes => Set<CollectionTheme>();
    public DbSet<ServiceItem> ServiceItems => Set<ServiceItem>();
    public DbSet<ServiceAssignment> ServiceAssignments => Set<ServiceAssignment>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<TopUpTransaction> TopUpTransactions => Set<TopUpTransaction>();
    public DbSet<QualificationRequest> QualificationRequests => Set<QualificationRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(x => x.Login).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Balance).HasColumnType("numeric(10,2)");
            entity.HasOne(x => x.EmployeeProfile)
                .WithOne(x => x.User)
                .HasForeignKey<Employee>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(x => x.Specialty).HasMaxLength(100);
            entity.Property(x => x.About).HasMaxLength(600);
        });

        modelBuilder.Entity<CollectionTheme>(entity =>
        {
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<ServiceItem>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(140);
            entity.Property(x => x.Price).HasColumnType("numeric(10,2)");
            entity.HasOne(x => x.CollectionTheme)
                .WithMany(x => x.Services)
                .HasForeignKey(x => x.CollectionThemeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ServiceAssignment>(entity =>
        {
            entity.HasKey(x => new { x.EmployeeId, x.ServiceItemId });
            entity.HasOne(x => x.Employee)
                .WithMany(x => x.ServiceAssignments)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ServiceItem)
                .WithMany(x => x.ServiceAssignments)
                .HasForeignKey(x => x.ServiceItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.CustomerBookings)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Employee)
                .WithMany(x => x.AssignedBookings)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ServiceItem)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.ServiceItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Booking)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.ServiceItem)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.ServiceItemId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TopUpTransaction>(entity =>
        {
            entity.Property(x => x.Amount).HasColumnType("numeric(10,2)");
            entity.HasOne(x => x.User)
                .WithMany(x => x.TopUps)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QualificationRequest>(entity =>
        {
            entity.HasOne(x => x.Employee)
                .WithMany(x => x.QualificationRequests)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override int SaveChanges()
    {
        NormalizeDateTimeOffsets();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        NormalizeDateTimeOffsets();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        NormalizeDateTimeOffsets();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        NormalizeDateTimeOffsets();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void NormalizeDateTimeOffsets()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is not EntityState.Added and not EntityState.Modified)
            {
                continue;
            }

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.ClrType == typeof(DateTimeOffset)
                    && property.CurrentValue is DateTimeOffset value)
                {
                    property.CurrentValue = value.ToUniversalTime();
                }
                else if (property.Metadata.ClrType == typeof(DateTimeOffset?)
                         && property.CurrentValue is DateTimeOffset nullableValue)
                {
                    property.CurrentValue = nullableValue.ToUniversalTime();
                }
            }
        }
    }
}
