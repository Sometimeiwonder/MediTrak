using MediTrack.Mvc.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Mvc.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<SupplyCategory> SupplyCategories => Set<SupplyCategory>();
    public DbSet<MedicalSupply> MediTrack => Set<MedicalSupply>();
    public DbSet<Issue> Issues => Set<Issue>();
    public DbSet<IssueItem> IssueItems => Set<IssueItem>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SupplyCategory>(entity =>
        {
            entity.ToTable("SupplyCategories");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<MedicalSupply>(entity =>
        {
            entity.ToTable("MediTrack");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Code).IsRequired().HasMaxLength(50);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(150);
            entity.Property(s => s.Supplier).HasMaxLength(100);
            entity.Property(s => s.UnitPrice).HasColumnType("decimal(18,2)");
            entity.HasOne(s => s.SupplyCategory)
                  .WithMany(c => c.Supplies)
                  .HasForeignKey(s => s.SupplyCategoryId);

            entity.HasIndex(s => s.Code).IsUnique();

            entity.HasQueryFilter(s => !s.IsDeleted);
        });

        modelBuilder.Entity<Issue>(entity =>
        {
            entity.ToTable("Issues");
            entity.HasKey(i => i.Id);
            entity.Property(i => i.IssuedTo).IsRequired().HasMaxLength(100);
            entity.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<IssueItem>(entity =>
        {
            entity.ToTable("IssueItems");
            entity.HasKey(ii => ii.Id);
            entity.Property(ii => ii.UnitPrice).HasColumnType("decimal(18,2)");
            entity.HasOne(ii => ii.Issue)
                  .WithMany(i => i.IssueItems)
                  .HasForeignKey(ii => ii.IssueId);
            entity.HasOne(ii => ii.MedicalSupply)
                  .WithMany()
                  .HasForeignKey(ii => ii.MedicalSupplyId);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Action).IsRequired().HasMaxLength(100);
            entity.Property(a => a.EntityName).IsRequired().HasMaxLength(100);
            entity.Property(a => a.EntityId).HasMaxLength(50);
            entity.Property(a => a.UserName).HasMaxLength(100);
            entity.Property(a => a.IpAddress).HasMaxLength(45);
            entity.Property(a => a.Result).HasMaxLength(50);
            entity.Property(a => a.Note).HasMaxLength(500);
            entity.HasIndex(a => a.CreatedAt);
            entity.HasIndex(a => a.UserName);
            entity.HasIndex(a => a.Action);
        });

        modelBuilder.Entity<SupplyCategory>().HasData(
            new SupplyCategory { Id = 1, Name = "Bảo hộ" },
            new SupplyCategory { Id = 2, Name = "Thiết bị kiểm tra" },
            new SupplyCategory { Id = 3, Name = "Tiêu hao" }
        );

        modelBuilder.Entity<MedicalSupply>().HasData(
            new MedicalSupply { Id = 1, Code = "MS-MSK-001", Name = "Khẩu trang y tế", SupplyCategoryId = 1, Supplier = "VinMed", UnitPrice = 1200, Quantity = 500, MinStock = 200, Description = "Khẩu trang y tế 3 lớp, phù hợp cho phòng khám", CreatedAt = new DateTime(2025, 5, 15, 8, 30, 0), IsDeleted = false },
            new MedicalSupply { Id = 2, Code = "MS-GLO-002", Name = "Găng tay cao su", SupplyCategoryId = 1, Supplier = "VietGlove", UnitPrice = 3400, Quantity = 180, MinStock = 200, Description = "Găng tay y tế không bột, size M/L", CreatedAt = new DateTime(2025, 5, 15, 9, 0, 0), IsDeleted = false },
            new MedicalSupply { Id = 3, Code = "MS-THE-003", Name = "Nhiệt kế hồng ngoại", SupplyCategoryId = 2, Supplier = "Omron Vietnam", UnitPrice = 320000, Quantity = 8, MinStock = 10, Description = "Nhiệt kế cầm tay đo nhiệt độ không tiếp xúc", CreatedAt = new DateTime(2025, 5, 15, 10, 0, 0), IsDeleted = false },
            new MedicalSupply { Id = 4, Code = "MS-BAN-004", Name = "Bông y tế", SupplyCategoryId = 3, Supplier = "Medicare", UnitPrice = 28000, Quantity = 0, MinStock = 15, Description = "Bông y tế tiêu khuẩn, gói 500g", CreatedAt = new DateTime(2025, 5, 15, 9, 30, 0), IsDeleted = false },
            new MedicalSupply { Id = 5, Code = "MS-SYR-005", Name = "Bơm tiêm 5ml", SupplyCategoryId = 3, Supplier = "Kim Tiêm Sài Gòn", UnitPrice = 1500, Quantity = 220, MinStock = 100, Description = "Bơm tiêm 5ml 1 lần sử dụng", CreatedAt = new DateTime(2025, 5, 15, 8, 45, 0), IsDeleted = false }
        );
    }
}
