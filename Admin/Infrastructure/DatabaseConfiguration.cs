﻿using Admin.Models.AppSettings;
using Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Admin.Infrastructure;

static class DatabaseConfiguration
{
	/// <summary>
	///     DbContext settings
	/// </summary>
	/// <param name="services"></param>
	/// <param name="configuration"></param>
	public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		IConfigurationSection databaseConnections = configuration.GetSection(nameof(DatabaseConnections));
		string connection = databaseConnections.GetValue<string>(nameof(DatabaseConnections.EmailDb));
		services.AddDbContext<ApplicationDbContext>(options =>
		{
			options.UseSqlServer(connection,
			  b => b.MigrationsAssembly($"{nameof(Database)}"));
		});
	}

	//TODO: Try removing was added to fix some docker issues
	public static void Configure(ApplicationDbContext dataContext)
	{
		// Apply migrations
		Console.WriteLine("Applying migrations....");
		dataContext.Database.Migrate();
		Console.WriteLine("Migrations applied....");

		// Seed database
		Console.WriteLine("Seeding database....");
		Console.WriteLine("TemplateTbl");
		if (dataContext.TemplateTbl.Any())
		{
			Console.WriteLine("Already have data - not seeding");
		}
		else
		{
			//Console.WriteLine("Adding data - seeding....");
			//dataContext.ProjectTbl.AddRange(
			//    new ProjectTbl
			//    {
			//        DateModified = DateTime.Now,
			//        Name = "Example",
			//        SubHeading = "This is a sub heading",
			//        Tags = "Test, Example",
			//        Templates = new List<TemplateTbl>
			//        {
			//            new TemplateTbl
			//            {
			//                Name = "Sign up confirmation",
			//            }
			//        }
			//    }
			//);
			//dataContext.SaveChanges();

			//Console.WriteLine("TemplateTbl - Seeding complete");
		}

		Console.WriteLine("Completed seeding");
	}
}