using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BusinessObjects.Models;

public partial class AMSDbContext : DbContext
{
    public AMSDbContext()
    {
    }

    public AMSDbContext(DbContextOptions<AMSDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Apartment> Apartments { get; set; }

    public virtual DbSet<ApartmentResident> ApartmentResidents { get; set; }

    public virtual DbSet<Building> Buildings { get; set; }

    public virtual DbSet<FeeType> FeeTypes { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceDetail> InvoiceDetails { get; set; }

    public virtual DbSet<Issue> Issues { get; set; }

    public virtual DbSet<IssueCategory> IssueCategories { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Resident> Residents { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public class ApartmentDbContext : DbContext
    {
        public ApartmentDbContext(DbContextOptions<ApartmentDbContext> options)
            : base(options)
        {
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Apartment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Apartmen__3214EC07AF1BF001");

            entity.HasIndex(e => e.ApartmentCode, "UQ__Apartmen__C6F22AE620D328C9").IsUnique();

            entity.Property(e => e.ApartmentCode).HasMaxLength(50);
            entity.Property(e => e.ApartmentType).HasMaxLength(50);
            entity.Property(e => e.Area).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.NumberOfRooms).HasDefaultValue(1);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Available");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Building).WithMany(p => p.Apartments)
                .HasForeignKey(d => d.BuildingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Apartment__Build__534D60F1");
        });

        modelBuilder.Entity<ApartmentResident>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Apartmen__3214EC07CABE1B4C");

            entity.HasIndex(e => new { e.ApartmentId, e.ResidentId, e.MoveInDate }, "UQ_AptResident_MoveIn").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(d => d.Apartment).WithMany(p => p.ApartmentResidents)
                .HasForeignKey(d => d.ApartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Apartment__Apart__619B8048");

            entity.HasOne(d => d.Resident).WithMany(p => p.ApartmentResidents)
                .HasForeignKey(d => d.ResidentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Apartment__Resid__628FA481");
        });

        modelBuilder.Entity<Building>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Building__3214EC07C567CE18");

            entity.HasIndex(e => e.BuildingCode, "UQ__Building__D4DA03244892EEE2").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.BuildingCode).HasMaxLength(20);
            entity.Property(e => e.BuildingName).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TotalFloors).HasDefaultValue(1);
        });

        modelBuilder.Entity<FeeType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FeeTypes__3214EC077A64F2D1");

            entity.HasIndex(e => e.FeeCode, "UQ__FeeTypes__CAD490B3B80DD650").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.FeeCode).HasMaxLength(50);
            entity.Property(e => e.FeeName).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Unit).HasMaxLength(50);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Invoices__3214EC0773A5C758");

            entity.HasIndex(e => new { e.ApartmentId, e.BillingMonth, e.BillingYear }, "UQ_Invoice_AptMonth").IsUnique();

            entity.HasIndex(e => e.InvoiceCode, "UQ__Invoices__0D9D7FF352ACDA10").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.InvoiceCode).HasMaxLength(50);
            entity.Property(e => e.IssueDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Unpaid");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Apartment).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.ApartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoices__Apartm__6FE99F9F");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoices__Create__76969D2E");

            entity.HasOne(d => d.Resident).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.ResidentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoices__Reside__70DDC3D8");
        });

        modelBuilder.Entity<InvoiceDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__InvoiceD__3214EC070D0E0DDA");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.FeeType).WithMany(p => p.InvoiceDetails)
                .HasForeignKey(d => d.FeeTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InvoiceDe__FeeTy__7C4F7684");

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceDetails)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("FK__InvoiceDe__Invoi__7B5B524B");
        });

        modelBuilder.Entity<Issue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Issues__3214EC0712CCF560");

            entity.HasIndex(e => e.IssueCode, "UQ__Issues__1CF9DA763C363D03").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ImageUrls).HasMaxLength(2000);
            entity.Property(e => e.IssueCode).HasMaxLength(50);
            entity.Property(e => e.Priority).HasDefaultValue(3);
            entity.Property(e => e.ResolutionNotes).HasMaxLength(2000);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Open");
            entity.Property(e => e.Title).HasMaxLength(500);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Apartment).WithMany(p => p.Issues)
                .HasForeignKey(d => d.ApartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Issues__Apartmen__0B91BA14");

            entity.HasOne(d => d.AssignedToUser).WithMany(p => p.IssueAssignedToUsers)
                .HasForeignKey(d => d.AssignedToUserId)
                .HasConstraintName("FK__Issues__Assigned__0E6E26BF");

            entity.HasOne(d => d.Category).WithMany(p => p.Issues)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Issues__Category__0C85DE4D");

            entity.HasOne(d => d.ReportedByUser).WithMany(p => p.IssueReportedByUsers)
                .HasForeignKey(d => d.ReportedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Issues__Reported__0D7A0286");
        });

        modelBuilder.Entity<IssueCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__IssueCat__3214EC071D9604C0");

            entity.Property(e => e.CategoryName).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.EstimatedResolutionDays).HasDefaultValue(3);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PriorityLevel).HasDefaultValue(3);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC07EB486807");

            entity.Property(e => e.Content).HasMaxLength(2000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Title).HasMaxLength(500);
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.RelatedInvoice).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.RelatedInvoiceId)
                .HasConstraintName("FK__Notificat__Relat__17F790F9");

            entity.HasOne(d => d.RelatedIssue).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.RelatedIssueId)
                .HasConstraintName("FK__Notificat__Relat__18EBB532");

            entity.HasOne(d => d.Sender).WithMany(p => p.NotificationSenders)
                .HasForeignKey(d => d.SenderId)
                .HasConstraintName("FK__Notificat__Sende__160F4887");

            entity.HasOne(d => d.User).WithMany(p => p.NotificationUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Notificat__UserI__151B244E");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payments__3214EC0796A32A4A");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.PaymentDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.ReferenceNumber).HasMaxLength(255);

            entity.HasOne(d => d.Invoice).WithMany(p => p.Payments)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payments__Invoic__00200768");

            entity.HasOne(d => d.ReceivedByNavigation).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ReceivedBy)
                .HasConstraintName("FK__Payments__Receiv__02084FDA");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RefreshT__3214EC0751BE61F0");

            entity.HasIndex(e => e.Token, "UQ__RefreshT__1EB4F81774A0BF05").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Token).HasMaxLength(512);

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__RefreshTo__UserI__47DBAE45");
        });

        modelBuilder.Entity<Resident>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Resident__3214EC07A3CA6BB0");

            // Bỏ IsUnique() ở đây vì đã xử lý bằng filtered index ở DB
            entity.HasIndex(e => e.UserId, "UQ_Residents_UserId").IsUnique();

            entity.HasIndex(e => e.CitizenId, "UQ__Resident__6E49FA0DE65CA61D").IsUnique();

            entity.Property(e => e.CitizenId).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.EmergencyContact).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            // Quan hệ optional: UserId nullable nên dùng HasOne/WithMany thay vì WithOne
            // Lý do: WithOne enforce 1-1 strict, sẽ lỗi khi UserId = NULL
            entity.HasOne(d => d.User)
                .WithMany(p => p.Residents)          // thay WithOne → WithMany
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)    // xóa User → set NULL, không xóa Resident
                .HasConstraintName("FK__Residents__UserI__5BE2A6F2");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC077F9C2E6B");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B6160C4E94306").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07C93DE32F");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E41A030310").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534E744F170").IsUnique();

            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(512);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserRole__3214EC07118FD026");

            entity.HasIndex(e => new { e.UserId, e.RoleId }, "UQ_UserRoles").IsUnique();

            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__UserRoles__RoleI__4316F928");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserRoles__UserI__4222D4EF");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
