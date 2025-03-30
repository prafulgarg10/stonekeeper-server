using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MyFirstServer.Models;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace MyFirstServer.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppConfig> AppConfigs { get; set; }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<EfmigrationsHistory> EfmigrationsHistories { get; set; }

    public virtual DbSet<Material> Materials { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderSummary> OrderSummaries { get; set; }

    public virtual DbSet<PricePerTenGram> PricePerTenGrams { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Role> Roles { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<AppConfig>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("AppConfig");

            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Value).HasMaxLength(255);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("AppUser");

            entity.HasIndex(e => e.RoleId, "Role_Id");

            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasColumnName("Is_Active");
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.RoleId).HasColumnName("Role_Id");
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Role).WithMany(p => p.AppUsers)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("appuser_ibfk_1");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Category");

            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasColumnName("Is_Active");
            entity.Property(e => e.Name).HasMaxLength(30);
            entity.Property(e => e.Purity).HasPrecision(5, 2);
        });

        modelBuilder.Entity<EfmigrationsHistory>(entity =>
        {
            entity.HasKey(e => e.MigrationId).HasName("PRIMARY");

            entity.ToTable("__EFMigrationsHistory");

            entity.Property(e => e.MigrationId).HasMaxLength(150);
            entity.Property(e => e.ProductVersion).HasMaxLength(32);
        });

        modelBuilder.Entity<Material>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Material");

            entity.Property(e => e.Name).HasMaxLength(10);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => e.CreatedBy, "fk_userId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("Created_At");
            entity.Property(e => e.CreatedBy).HasColumnName("Created_By");
            entity.Property(e => e.Total).HasPrecision(12, 2);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_userId");
        });

        modelBuilder.Entity<OrderSummary>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.ProductId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("OrderSummary");

            entity.HasIndex(e => e.MaterialPriceId, "Material_Price_Id");

            entity.HasIndex(e => e.ProductId, "Product_Id");

            entity.Property(e => e.ProductId).HasColumnName("Product_Id");
            entity.Property(e => e.MaterialPriceId).HasColumnName("Material_Price_Id");
            entity.Property(e => e.ProductCategoryId).HasColumnName("Product_Category_Id");
            entity.Property(e => e.ProductQuantity).HasColumnName("Product_Quantity");
            entity.Property(e => e.ProductTotal)
                .HasPrecision(10, 2)
                .HasColumnName("Product_Total");
            entity.Property(e => e.ProductWeight)
                .HasPrecision(10, 2)
                .HasColumnName("Product_Weight");

            entity.HasOne(d => d.IdNavigation).WithMany(p => p.OrderSummaries)
                .HasForeignKey(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ordersummary_ibfk_1");

            entity.HasOne(d => d.MaterialPrice).WithMany(p => p.OrderSummaries)
                .HasForeignKey(d => d.MaterialPriceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ordersummary_ibfk_3");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderSummaries)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ordersummary_ibfk_2");
        });

        modelBuilder.Entity<PricePerTenGram>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => e.MaterialId, "Material_Id");

            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.MaterialId).HasColumnName("Material_Id");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Product");

            entity.HasIndex(e => e.CategoryId, "Category_Id");

            entity.HasIndex(e => e.MaterialId, "Material_Id");

            entity.Property(e => e.CategoryId).HasColumnName("Category_Id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("Created_At");
            entity.Property(e => e.ImageName)
                .HasMaxLength(50)
                .HasColumnName("image_name");
            entity.Property(e => e.IsActive).HasColumnName("Is_Active");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.MaterialId).HasColumnName("Material_Id");
            entity.Property(e => e.Name).HasMaxLength(30);
            entity.Property(e => e.ProductImage)
                .HasColumnType("mediumblob")
                .HasColumnName("product_image");
            entity.Property(e => e.Weight).HasPrecision(10, 2);

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("product_ibfk_2");

            entity.HasOne(d => d.Material).WithMany(p => p.Products)
                .HasForeignKey(d => d.MaterialId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("product_ibfk_1");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.IsActive).HasColumnName("Is_Active");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
