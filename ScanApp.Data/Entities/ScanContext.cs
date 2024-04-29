using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ScanApp.Data.Entities;

public partial class ScanContext : DbContext
{
    public ScanContext()
    {
    }

    public ScanContext(DbContextOptions<ScanContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Batch> Batches { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = @"Data Source=C:\Code\LibraSoft\ScanProject\Oriscan\OriginalScan\scan.db";
        optionsBuilder.UseSqlite(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Batch>(entity =>
        {
            entity.ToTable("batch");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BatchName).HasColumnName("batch_name");
            entity.Property(e => e.BatchPath).HasColumnName("batch_path");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.Note).HasColumnName("note");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("document");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BatchId).HasColumnName("batch_id");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.DocumentName).HasColumnName("document_name");
            entity.Property(e => e.DocumentPath).HasColumnName("document_path");
            entity.Property(e => e.Note).HasColumnName("note");

            entity.HasOne(d => d.Batch).WithMany(p => p.Documents)
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.ToTable("image");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.ImageName).HasColumnName("image_name");
            entity.Property(e => e.ImagePath).HasColumnName("image_path");

            entity.HasOne(d => d.Document).WithMany(p => p.Images)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
