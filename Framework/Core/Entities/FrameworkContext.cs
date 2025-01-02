using Microsoft.EntityFrameworkCore;

namespace Service.Framework.Core.Entities;

public partial class FrameworkContext : DbContext
{
  public FrameworkContext()
  {
  }

  public FrameworkContext(DbContextOptions<FrameworkContext> options)
    : base(options)
  {
  }

  public virtual DbSet<Config> Configs { get; set; }

  public virtual DbSet<Text> Texts { get; set; }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https: //go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
  {
    var currentDirectory = Directory.GetCurrentDirectory();
    var connectionString = $"Data Source={currentDirectory}/framework.sqlite;Cache=Shared;";
    optionsBuilder.UseSqlite(connectionString);
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Config>(entity =>
    {
      entity.ToTable("config");

      entity.Property(e => e.Id).HasColumnName("id");
      entity.Property(e => e.Name).HasColumnName("name");
      entity.Property(e => e.Value).HasColumnName("value");
    });

    modelBuilder.Entity<Text>(entity =>
    {
      entity.ToTable("text");

      entity.Property(e => e.Id).HasColumnName("id");
      entity.Property(e => e.Key).HasColumnName("key");
      entity.Property(e => e.Locale).HasColumnName("locale");
      entity.Property(e => e.Used).HasColumnName("used");
      entity.Property(e => e.Value).HasColumnName("value");
    });

    OnModelCreatingPartial(modelBuilder);
  }

  partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
