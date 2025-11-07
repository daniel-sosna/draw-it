using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Draw.it.Server.Models.User;
using Draw.it.Server.Models.Room;

namespace Draw.it.Server.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<UserModel> Users => Set<UserModel>();
    public DbSet<RoomModel> Rooms => Set<RoomModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global sequence for user ids (keeps parity with GetNextId semantics)
        modelBuilder.HasSequence<long>("user_id_seq")
            .HasMin(0)
            .StartsAt(0)
            .IncrementsBy(1);

        // Users
        var users = modelBuilder.Entity<UserModel>();
        users.ToTable("users");
        users.HasKey(u => u.Id);
        users.Property(u => u.Id).ValueGeneratedNever();
        users.Property(u => u.Name).IsRequired();
        users.Property(u => u.RoomId).HasMaxLength(64);
        users.HasIndex(u => u.RoomId);

        // Make RoomId a real FK to rooms(Id); when a room is deleted, null-out RoomId
        users
            .HasOne<RoomModel>()
            .WithMany()
            .HasForeignKey(u => u.RoomId)
            .OnDelete(DeleteBehavior.SetNull);

        // Rooms
        var rooms = modelBuilder.Entity<RoomModel>();
        rooms.ToTable("rooms");
        rooms.HasKey(r => r.Id);
        rooms.Property(r => r.Id).HasMaxLength(64);
        rooms.Property(r => r.HostId).IsRequired();

        // Map RoomSettingsModel into flat columns as an owned type
        rooms.OwnsOne(r => r.Settings, s =>
        {
            s.Property(x => x.RoomName).HasColumnName("RoomName");
            s.Property(x => x.CategoryId).HasColumnName("CategoryId");
            s.Property(x => x.DrawingTime).HasColumnName("DrawingTime");
            s.Property(x => x.NumberOfRounds).HasColumnName("NumberOfRounds");
        });
    }
}
