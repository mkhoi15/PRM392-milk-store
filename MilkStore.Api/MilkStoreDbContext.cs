using Microsoft.EntityFrameworkCore;
using MilkStore.Api.Models;
using MilkStore.Api.Shared.Enums;

namespace MilkStore.Api;

public class MilkStoreDbContext : DbContext
{
    public MilkStoreDbContext()
    {
    }
    
    public MilkStoreDbContext(DbContextOptions<MilkStoreDbContext> options) : base(options)
    {
    }
    
    public DbSet<Store> Stores { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Brand> Brands { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Store>().ToTable("Stores")
            .HasKey(s => s.Id);

        modelBuilder.Entity<User>().ToTable("Users")
            .HasKey(u => u.Id);
        modelBuilder.Entity<User>()
            .HasMany<Order>(u => u.Orders)
            .WithOne(o => o.User)
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Product>().ToTable("Products")
            .HasKey(p => p.Id);
        modelBuilder.Entity<Product>()
            .HasMany<OrderDetail>(p => p.OrderDetails)
            .WithOne(od => od.Product)
            .HasForeignKey(od => od.ProductId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Order>().ToTable("Orders")
            .HasMany<OrderDetail>(o => o.OrderDetails)
            .WithOne(od => od.Order)
            .HasForeignKey(od => od.OrderId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<OrderDetail>().ToTable("OrderDetails")
            .HasKey(od => od.Id);
        
        modelBuilder.Entity<Role>().ToTable("Roles")
            .HasKey(r => r.Id);
        modelBuilder.Entity<Role>()
            .HasMany<User>(r => r.Users)
            .WithOne(u => u.Role)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.NoAction);
        
        modelBuilder.Entity<Brand>().ToTable("Brands")
            .HasKey(b => b.Id);
        modelBuilder.Entity<Brand>()
            .HasMany<Product>(b => b.Products)
            .WithOne(p => p.Brand)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.NoAction);
        
        // Seed data
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = Guid.NewGuid(), Name = RoleName.Admin.ToString() },
            new Role { Id = Guid.NewGuid(), Name = RoleName.Owner.ToString() },
            new Role { Id = Guid.NewGuid(), Name = RoleName.ShopStaff.ToString() },
            new Role { Id = Guid.NewGuid(), Name = RoleName.DeliveryStaff.ToString() },
            new Role { Id = Guid.NewGuid(), Name = RoleName.Customer.ToString() }
        );
    }
}