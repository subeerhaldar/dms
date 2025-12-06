using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Dms.Core.Entities;
using Dms.Infrastructure.Entities;

namespace Dms.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentVersion> DocumentVersions { get; set; }
    public DbSet<DocumentMetadata> DocumentMetadata { get; set; }
    public DbSet<WorkflowState> WorkflowStates { get; set; }
    public DbSet<DocumentPermission> DocumentPermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure entities
        builder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.CreatedBy).HasMaxLength(450); // Identity User Id
            entity.Property(e => e.ModifiedBy).HasMaxLength(450);
        });

        builder.Entity<DocumentVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.CreatedBy).HasMaxLength(450);
            entity.HasOne(e => e.Document)
                .WithMany(d => d.Versions)
                .HasForeignKey(e => e.DocumentId);
        });

        builder.Entity<DocumentMetadata>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Value).HasMaxLength(1000);
            entity.HasOne(e => e.Document)
                .WithMany(d => d.Metadata)
                .HasForeignKey(e => e.DocumentId);
        });

        builder.Entity<WorkflowState>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
        });

        builder.Entity<DocumentPermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(450);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.HasOne(e => e.Document)
                .WithMany(d => d.Permissions)
                .HasForeignKey(e => e.DocumentId);
        });

        // Seed data for WorkflowStates
        builder.Entity<WorkflowState>().HasData(
            new WorkflowState { Id = 1, Name = "Pending" },
            new WorkflowState { Id = 2, Name = "Approved" },
            new WorkflowState { Id = 3, Name = "Rejected" }
        );
    }
}