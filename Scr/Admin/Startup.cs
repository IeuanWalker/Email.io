using System.Reflection;
using Admin.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

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
		// App settings
		AppSettingsConfiguration.ConfigureServices(services, Configuration);

		services.AddMemoryCache();

		// Interface mapping
		InterfaceConfiguration.ConfigureServices(services);

		// Database
		DatabaseConfiguration.ConfigureServices(services, Configuration);

		// AutoMapper
		services.AddAutoMapper(typeof(Startup).GetTypeInfo().Assembly);

		// Hangfire
		HangfireConfiguration.ConfigureServices(services, Configuration);

		// Authentication - https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/additional-claims?view=aspnetcore-3.1
		services.AddAuthentication(options => {
			options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
		})
		.AddCookie()
		.AddOpenIdConnect(o =>
		{
			o.ClientId = "";
			o.ClientSecret = "";
			o.Authority = "https://.onelogin.com/oidc";
			o.ResponseType = "code";
			o.GetClaimsFromUserInfoEndpoint = true;
		});

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

		app.UseStatusCodePagesWithReExecute("/Error{0}");

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