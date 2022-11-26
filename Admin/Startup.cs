using System.Reflection;
using Admin.Infrastructure;
using Database.Context;

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

		// AutoMapper
		services.AddAutoMapper(typeof(Startup).GetTypeInfo().Assembly);

		// Hangfire
		HangfireConfiguration.ConfigureServices(services, Configuration);

		services.AddRazorPages()
			.AddRazorRuntimeCompilation();
	}

	// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

		app.Use(async (ctx, next) =>
		{
			await next().ConfigureAwait(false);

			if (ctx.Response.StatusCode == 404 && !ctx.Response.HasStarted)
			{
				//Re-execute the request so the user gets the error page
				ctx.Items["originalPath"] = ctx.Request.Path.Value;
				ctx.Request.Path = "/Error404";
				await next().ConfigureAwait(false);
			}
		});

		app.UseStatusCodePagesWithReExecute("/Error");

		DatabaseConfiguration.Configure(app);

		// Hangfire
		HangfireConfiguration.Configure(app);

		app.UseHttpsRedirection();
		app.UseStaticFiles();

		app.UseRouting();

		app.UseAuthorization();

		app.UseEndpoints(endpoints => endpoints.MapRazorPages());

		
	}
}