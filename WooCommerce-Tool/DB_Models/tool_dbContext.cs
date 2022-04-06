using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace WooCommerce_Tool.DB_Models
{
    public partial class tool_dbContext : DbContext
    {
        public tool_dbContext()
        {
        }

        public tool_dbContext(DbContextOptions<tool_dbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ToolLogin> ToolLogins { get; set; } = null!;
        public virtual DbSet<ToolOrder> ToolOrders { get; set; } = null!;
        public virtual DbSet<ToolProduct> ToolProducts { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("server=DESKTOP-KV5D8QG\\SQLEXPRESS;database=tool_db;Integrated Security=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ToolLogin>(entity =>
            {
                entity.ToTable("Tool_login");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("ID");

                entity.Property(e => e.ApiKey).HasColumnName("API_key");

                entity.Property(e => e.ApiSecret).HasColumnName("API_secret");

                entity.Property(e => e.Url).HasColumnName("URL");
            });

            modelBuilder.Entity<ToolOrder>(entity =>
            {
                entity.ToTable("Tool_order");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("ID");

                entity.Property(e => e.EndDate).HasColumnName("End_Date");

                entity.Property(e => e.NnOrder).HasColumnName("NN_order");

                entity.Property(e => e.ProbabilityTimeOfTheDay).HasColumnName("Probability_TimeOfTheDay");

                entity.Property(e => e.ProbabilityTimeOfTheMonth).HasColumnName("Probability_TimeOfTheMonth");

                entity.Property(e => e.RegresionOrder).HasColumnName("Regresion_order");

                entity.Property(e => e.ShopId).HasColumnName("Shop_ID");

                entity.Property(e => e.StartDate).HasColumnName("Start_Date");

                entity.Property(e => e.TimeSeriesOrder).HasColumnName("TimeSeries_order");

                entity.Property(e => e.TotalOrder).HasColumnName("Total_order");

                entity.HasOne(d => d.Shop)
                    .WithMany(p => p.ToolOrders)
                    .HasForeignKey(d => d.ShopId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tool_order_Tool_login");
            });

            modelBuilder.Entity<ToolProduct>(entity =>
            {
                entity.ToTable("Tool_product");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("ID");

                entity.Property(e => e.EndDate).HasColumnName("End_date");

                entity.Property(e => e.NnProducts).HasColumnName("NN_products");

                entity.Property(e => e.ProbabilityCategory).HasColumnName("Probability_category");

                entity.Property(e => e.ProbabilityProducts).HasColumnName("Probability_products");

                entity.Property(e => e.RegresionProducts).HasColumnName("Regresion_products");

                entity.Property(e => e.ShopId).HasColumnName("Shop_ID");

                entity.Property(e => e.StartDate).HasColumnName("Start_date");

                entity.Property(e => e.TimeSeriesProducts).HasColumnName("TimeSeries_products");

                entity.Property(e => e.TotalProducts).HasColumnName("Total_products");

                entity.HasOne(d => d.Shop)
                    .WithMany(p => p.ToolProducts)
                    .HasForeignKey(d => d.ShopId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tool_product_Tool_login");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
