using Admin.Infrastructure;
using Database.Context;
using Newtonsoft.Json.Serialization;
using Westwind.AspNetCore.LiveReload;

namespace Admin;

public class Startup
{
	public Startup(IConfiguration configuration)
	{
		Configuration = configuration;
	}

	public IConfiguration Configuration { get; }

	// This method gets called by the runtime. Use this method to add services to the container.
	public void ConfigureServices(IServiceCollection services)
	{
		services.AddMemoryCache();

		// Interface mapping
		InterfaceConfiguration.ConfigureServices(services);

		// Database
		DatabaseConfiguration.ConfigureServices(services, Configuration);

		// Hangfire
		HangfireConfiguration.ConfigureServices(services, Configuration);

		services.AddRazorPages()
			.AddRazorRuntimeCompilation();
	}

	// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
	public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ApplicationDbContext dataContext)
	{
		if (env.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}
		else
		{
			app.UseExceptionHandler("/Error");
			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			app.UseHsts();
		}

		// Hangfire
		HangfireConfiguration.Configure(app);

		app.UseHttpsRedirection();
		app.UseStaticFiles();

		app.UseRouting();

		app.UseAuthorization();

		app.UseEndpoints(endpoints => endpoints.MapRazorPages());

		DatabaseConfiguration.Configure(dataContext);
	}
}