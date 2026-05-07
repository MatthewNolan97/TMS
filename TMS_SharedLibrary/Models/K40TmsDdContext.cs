using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TMS_SharedLibrary.Models;

public partial class K40TmsDdContext : DbContext
{
    public K40TmsDdContext()
    {
    }

    public K40TmsDdContext(DbContextOptions<K40TmsDdContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    public virtual DbSet<Toy> Toys { get; set; }

    public virtual DbSet<ToyLoan> ToyLoans { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        if (!optionsBuilder.IsConfigured) {
            optionsBuilder.UseSqlServer("Server=tcp:csdevdb-heritagecs.database.windows.net,1433;Database=K50_TMS_TEST;Authentication=Active Directory Interactive;Encrypt=True;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__4BA5CE8975875DBC");

            entity.ToTable("Notification");

            entity.Property(e => e.NotificationId).HasColumnName("notificationID");
            entity.Property(e => e.DateSent)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("dateSent");
            entity.Property(e => e.Message)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("message");
            entity.Property(e => e.RecipientId).HasColumnName("recipientID");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("type");

            entity.HasOne(d => d.Recipient).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.RecipientId)
                .HasConstraintName("FK__Notificat__recip__02084FDA");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Student__4D11D65C30690382");

            entity.ToTable("Student");

            entity.HasIndex(e => e.UserId, "UQ__Student__CB9A1CDE8CA0A031").IsUnique();

            entity.Property(e => e.StudentId).HasColumnName("studentID");
            entity.Property(e => e.Placement)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("placement");
            entity.Property(e => e.UserId).HasColumnName("userID");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.User).WithOne(p => p.Student)
                .HasForeignKey<Student>(d => d.UserId)
                .HasConstraintName("FK__Student__userID__02FC7413");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.TeacherId).HasName("PK__Teacher__98E938751A58C5D3");

            entity.ToTable("Teacher");

            entity.HasIndex(e => e.UserId, "UQ__Teacher__CB9A1CDE72F16825").IsUnique();

            entity.Property(e => e.TeacherId).HasColumnName("teacherID");
            entity.Property(e => e.UserId).HasColumnName("userID");

            entity.HasOne(d => d.User).WithOne(p => p.Teacher)
                .HasForeignKey<Teacher>(d => d.UserId)
                .HasConstraintName("FK__Teacher__userID__03F0984C");
        });

        modelBuilder.Entity<Toy>(entity =>
        {
            entity.HasKey(e => e.ToyId).HasName("PK__Toy__CE1016FD1D0C2A3A");

            entity.ToTable("Toy");

            entity.Property(e => e.ToyId).HasColumnName("toyID");
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("category");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("imagePath");
            entity.Property(e => e.IsAvailable)
                .HasDefaultValue(true)
                .HasColumnName("isAvailable");
            entity.Property(e => e.LocationCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("locationCode");
            entity.Property(e => e.ManagedBy).HasColumnName("managedBy");
            entity.Property(e => e.Material)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("material");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("name");

            entity.HasOne(d => d.ManagedByNavigation).WithMany(p => p.Toys)
                .HasForeignKey(d => d.ManagedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Toy__managedBy__04E4BC85");
        });

        modelBuilder.Entity<ToyLoan>(entity =>
        {
            entity.HasKey(e => e.LoanId).HasName("PK__ToyLoan__6DB7891FF19B3436");

            entity.ToTable("ToyLoan");

            entity.Property(e => e.LoanId).HasColumnName("loanID");
            entity.Property(e => e.BorrowDate).HasColumnName("borrowDate");
            entity.Property(e => e.DueDate).HasColumnName("dueDate");
            entity.Property(e => e.ReturnDate).HasColumnName("returnDate");
            entity.Property(e => e.StudentId).HasColumnName("studentID");
            entity.Property(e => e.ToyId).HasColumnName("toyID");


            entity.HasOne(d => d.Student).WithMany(p => p.ToyLoans)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ToyLoan__student__07C12930");

            entity.HasOne(d => d.Toy).WithMany(p => p.ToyLoans)
                .HasForeignKey(d => d.ToyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ToyLoan__toyID__08B54D69");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__CB9A1CDFBA39697B");

            entity.ToTable("User", t => t.ExcludeFromMigrations()
    .HasTrigger("trg_UserInsertRouter"));

            entity.Property(e => e.UserId).HasColumnName("userID");
            entity.Property(e => e.UserType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("userType");
            entity.Property(e => e.Oid)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("oid");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
