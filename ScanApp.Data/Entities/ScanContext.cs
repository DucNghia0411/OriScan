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
        string connectionString = @"Data Source=D:\GitLab\OriScan\OriginalScan\scan.db";

        //ConnectionString for app packing, to file debug
        //string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scan.db");
        //string connectionString = @"Data Source=" + dbPath;

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
            entity.Property(e => e.NumberingFont).HasColumnName("numbering_font");
            entity.Property(e => e.DocumentRack).HasColumnName("document_rack");
            entity.Property(e => e.DocumentShelf).HasColumnName("document_shelf");
            entity.Property(e => e.NumericalTableOfContents).HasColumnName("numerical_table_of_contents");
            entity.Property(e => e.FileCabinet).HasColumnName("file_cabinet");
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
            entity.Property(e => e.AgencyIdentifier).HasColumnName("agency_identifier");
            entity.Property(e => e.DocumentIdentifier).HasColumnName("document_identifier");
            entity.Property(e => e.NumberOfSheets).HasColumnName("number_of_sheets");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.StoragePeriod).HasColumnName("storage_period");
            entity.Property(e => e.PdfPath).HasColumnName("pdf_path");

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
            entity.Property(e => e.Order).HasColumnName("order");
            entity.HasOne(d => d.Document).WithMany(p => p.Images)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<DeviceSetting>(entity =>
        {
            entity.ToTable("device_setting");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SettingName).HasColumnName("setting_name");
            entity.Property(e => e.DeviceName).HasColumnName("device_name");
            entity.Property(e => e.IsDuplex).HasColumnName("is_duplex");
            entity.Property(e => e.Size).HasColumnName("size");
            entity.Property(e => e.Dpi).HasColumnName("dpi");
            entity.Property(e => e.PixelType).HasColumnName("pixel_type");
            entity.Property(e => e.BitDepth).HasColumnName("bit_depth");
            entity.Property(e => e.RotateDegree).HasColumnName("rotate_degree");
            entity.Property(e => e.Brightness).HasColumnName("brightness");
            entity.Property(e => e.Contrast).HasColumnName("contrast");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
        });
        
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
