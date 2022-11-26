using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Database.Context;

public class ApplicationDbContext : DbContext
{
	public ApplicationDbContext(DbContextOptions options) : base(options)
	{
	}

	public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
	{
		public ApplicationDbContext CreateDbContext(string[] args)
		{
			IConfigurationRoot configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.Build();

			IConfigurationSection databaseConnections = configuration.GetSection("DatabaseConnections");
			DbContextOptionsBuilder<ApplicationDbContext> builder = new();
			builder.UseSqlServer(databaseConnections.GetValue<string>("EmailDb"));
			return new ApplicationDbContext(builder.Options);
		}
	}

	/// <inheritdoc />
	/// <summary>
	///     Overrides EF SaveChanges method for auditing purposes
	/// </summary>
	public override int SaveChanges()
	{
		Auditing();
		return base.SaveChanges();
	}

	/// <summary>
	///     Adds auditing information to entity
	/// </summary>
	void Auditing()
	{
		foreach (EntityEntry entry in ChangeTracker.Entries())
		{
			if (entry.Entity is BaseEntityModifiedDate baseModifiedDateEntity)
			{
				DateTime now = DateTime.UtcNow;
				switch (entry.State)
				{
					case EntityState.Modified:
						baseModifiedDateEntity.DateModified = now;
						break;

					case EntityState.Added:
						baseModifiedDateEntity.DateModified = now;
						break;
				}
			}
		}
	}

	#region DbSet's

	public DbSet<ProjectTbl> ProjectTbl => Set<ProjectTbl>();
	public DbSet<EmailTbl> EmailTbl => Set<EmailTbl>();
	public DbSet<EmailAddressTbl> EmailAddressTbl => Set<EmailAddressTbl>();
	public DbSet<EmailAttachmentTbl> EmailAttachmentTbl => Set<EmailAttachmentTbl>();
	public DbSet<TemplateTbl> TemplateTbl => Set<TemplateTbl>();
	public DbSet<TemplateVersionTbl> TemplateVersionTbl => Set<TemplateVersionTbl>();

	#endregion DbSet's
}