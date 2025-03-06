using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace EBCEYS.Server_configuration.ConfigDatabase
{
    /// <summary>
    /// A <see cref="ConfigurationDatabaseContext"/> class.
    /// </summary>
    /// <remarks>
    /// Initiates a new instance of <see cref="ConfigurationDatabaseContext"/>.
    /// </remarks>
    /// <param name="opts">The db context options.</param>
    public class ConfigurationDatabaseContext(DbContextOptions<ConfigurationDatabaseContext> opts) : DbContext(opts)
    {
        /// <summary>
        /// The containers.
        /// </summary>
        [NotNull]
        public virtual DbSet<ContainerDBEntity> Containers { get; set; }
        /// <summary>
        /// The configs.
        /// </summary>
        [NotNull]
        public virtual DbSet<ConfigurationDBEntity> Configs { get; set; }
        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContainerDBEntity>(cent =>
            {
                cent.HasKey(c => c.Id);
                cent.HasMany(c => c.Configs).WithOne(c => c.Container).HasForeignKey(c => c.ContainerId).OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<ConfigurationDBEntity>(conf =>
            {
                conf.HasKey(c => c.FilePath);
                conf.HasOne(c => c.Container).WithMany(c => c.Configs).HasForeignKey(c => c.ContainerId).OnDelete(DeleteBehavior.ClientCascade);
            });
            base.OnModelCreating(modelBuilder);
        }
    }

    /// <summary>
    /// A <see cref="ConfigurationDBEntity"/> class.
    /// </summary>
    [Table("configuration_files")]
    public class ConfigurationDBEntity
    {
        /// <summary>
        /// The full file path at the system.
        /// </summary>
        [Key]
        [Required]
        [Column("file_path")]
        public string FilePath { get; set; } = null!;
        /// <summary>
        /// The container.
        /// </summary>
        [ForeignKey(nameof(ContainerId))]
        public ContainerDBEntity Container { get; set; } = null!;
        /// <summary>
        /// The container id.
        /// </summary>
        [Column("container_id")]
        [Required]
        public string ContainerId { get; set; } = null!;
        /// <summary>
        /// The container file path.
        /// </summary>
        [Column("container_file_path")]
        [Required]
        public string ContainerFilePath { get; set; } = null!;
        /// <summary>
        /// The file MTime.
        /// </summary>
        [Column("file_m_time")]
        [Required]
        public DateTime FileMTime { get; set; }
        /// <summary>
        /// The file last update.
        /// </summary>
        [Column("file_last_update")]
        [Required]
        public DateTime FileLastUpdate { get; set; }
        /// <summary>
        /// Is file exists.
        /// </summary>
        [Column("is_exists")]
        [Required]
        public bool IsExists { get; set; }
    }
    /// <summary>
    /// A <see cref="ContainerDBEntity"/> class.
    /// </summary>
    [Table("containers")]
    public class ContainerDBEntity
    {
        /// <summary>
        /// The container id.
        /// </summary>
        [Key]
        [Required]
        [Column("id")]
        public string Id { get; set; } = null!;
        /// <summary>
        /// The container name.
        /// </summary>
        [Column("name")]
        [Required]
        public string Name { get; set; } = null!;
        /// <summary>
        /// The container configuration path.
        /// </summary>
        [Column("configuration_path")]
        [Required]
        public string ConfigurationPath { get; set; } = null!;
        /// <summary>
        /// The last config set <see cref="DateTimeOffset"/> at UTC.
        /// </summary>
        [Column("last_config_set_UTC")]
        [Required]
        public DateTimeOffset LastConfigSetUTC { get; set; }
        /// <summary>
        /// The container configs.
        /// </summary>
        public ICollection<ConfigurationDBEntity> Configs { get; set; } = [];
        /// <summary>
        /// Is container exists in docker.
        /// </summary>
        [Column("is_exists")]
        [Required]
        public bool IsExists { get; set; }
        /// <summary>
        /// The container deletion datetime at UTC.
        /// </summary>
        [Column("marked_for_deletion_UTC")]
        public DateTimeOffset? DeletionUTC { get; set; } = null;
    }
}
