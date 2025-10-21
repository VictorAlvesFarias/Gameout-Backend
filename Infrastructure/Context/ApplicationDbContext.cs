using Domain.Entitites;
using Domain.Entitites.ApplicationContextDb;
using Domain.Entitites.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Infrastructure.Context
{
    public class ApplicationDbContext : IdentityDbContext<BaseEntityIdentity>
    {
        public readonly IHttpContextAccessor _contextAccessor;

        public DbSet<AppFile> AppFile { get; set; }
        public DbSet<AppStoredFile> AppStoredFile { get; set; }
        public DbSet<StoredFile> StoredFile { get; set; }
        public DbSet<AppFileLog> AppFileLog { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor contextAccessor) : base(options)
        {
            _contextAccessor = contextAccessor;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppStoredFile>(entity =>
            {
                entity.ToTable("AppStoredFiles");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.AppFileId)
                      .IsRequired();

                entity.Property(x => x.StoredFileId)
                      .IsRequired(false);

                entity.Property(x => x.Versioned)
                      .IsRequired();

                entity.Property(x => x.Processing)
                      .IsRequired();

                entity.Property(x => x.Error)
                      .HasMaxLength(1024)
                      .IsRequired(false);

                entity.Property(x => x.Message)
                      .HasMaxLength(1024)
                      .IsRequired(false);

                entity.Property(x => x.UpdateDate)
                      .IsRequired();

                entity.HasOne(x => x.AppFile)
                      .WithMany()
                      .HasForeignKey(x => x.AppFileId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired(true);

                entity.HasOne(x => x.StoredFile)
                      .WithMany()
                      .HasForeignKey(x => x.StoredFileId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);
            });

            modelBuilder.Entity<AppFileLog>(entity =>
            {
                entity.ToTable("AppFileLogs");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Path)
                      .HasMaxLength(500)
                      .IsRequired(false);

                entity.Property(x => x.RecordName)
                      .HasMaxLength(200)
                      .IsRequired(false);

                entity.Property(x => x.ActionMessage)
                      .HasMaxLength(1000)
                      .IsRequired();

                entity.Property(x => x.ActionType)
                      .IsRequired();

                entity.Property(x => x.CreateDate)
                      .IsRequired();

                entity.Property(x => x.UpdateDate)
                      .IsRequired();

            });
        }

        public string GetUserId()
        {
            return _contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
