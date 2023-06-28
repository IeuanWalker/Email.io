using System.Security.Claims;
using Admin.Infrastructure;
using Database.Models;
using Database.Repositories.User;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web.UI;

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
		services.AddScoped<AuthorizationCodeHandler>();

		// Database
		DatabaseConfiguration.ConfigureServices(services, Configuration);

		services
			.AddAuthentication(options =>
			{
				options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
			})
			.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
			.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
			{
				options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				options.Authority = "https://login.microsoftonline.com/6d2b6129-6234-4f81-932c-25af126af273/";
				options.ClientId = "39e54bdf-ef10-490e-92e3-aa8c3439a2b1";
				options.ClientSecret = "4ts8Q~KPM~BXdKs7drrN6fnD3hIUA7YfzW4KbbY0";
				options.ResponseType = "Code";
				options.UsePkce = true;
				options.Scope.Clear();
				options.Scope.Add("openid");
				options.Scope.Add("profile");
				options.SaveTokens = true;
				options.GetClaimsFromUserInfoEndpoint = true;
				options.Events = new OpenIdConnectEvents
				{
					OnTokenValidated = async context =>
					{
						AuthorizationCodeHandler codeHandler = context.HttpContext.RequestServices.GetRequiredService<AuthorizationCodeHandler>();
						await codeHandler.HandleAuthorizationCodeAsync(context);
					}
				};
			});
		services.AddAuthorization();

		services.AddControllersWithViews().AddMicrosoftIdentityUI();

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

		app.UseStatusCodePagesWithReExecute("/Error{0}");

		DatabaseConfiguration.Configure(app);

		app.UseHttpsRedirection();
		app.UseStaticFiles();

		app.UseRouting();

		app.UseAuthentication();
		app.UseAuthorization();

		// Hangfire
		HangfireConfiguration.Configure(app);

		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllers();
			endpoints.MapRazorPages();
		});
	}
}

public class AuthorizationCodeHandler
{
	readonly IUserRepository _userTbl;

	public AuthorizationCodeHandler(IUserRepository userTbl)
	{
		_userTbl = userTbl ?? throw new ArgumentNullException(nameof(userTbl));
	}

	public async Task HandleAuthorizationCodeAsync(TokenValidatedContext context)
	{
		if (context.Principal is null)
		{
			throw new NullReferenceException("Claim principal is null");
		}

		string userId = context.Principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? context.Principal.FindFirstValue("sub") ?? throw new NullReferenceException("User ID claim not found");
		string email = context.Principal.FindFirstValue(ClaimTypes.Email) ?? throw new NullReferenceException("User email claim not found");
		string? givenName = context.Principal.FindFirstValue(ClaimTypes.GivenName);
		string? familyName = context.Principal.FindFirstValue(ClaimTypes.Surname);
		string displayName = givenName ?? email;
		string initials = GetInstials(givenName, familyName, email).ToUpper();

		UserTbl? user = _userTbl.Where(u => u.Sub == userId).FirstOrDefault();

		if (user is not null)
		{
			bool needUpdating = false;

			if (!user.Email.Equals(email))
			{
				user.Email = email;
				needUpdating = true;
			}

			if (givenName is not null && user.GivenName != givenName)
			{
				user.GivenName = givenName;
				needUpdating = true;
			}

			if (familyName is not null && user.FamilyName != familyName)
			{
				user.FamilyName = familyName;
				needUpdating = true;
			}

			if (!user.DisplayName.Equals(displayName))
			{
				user.DisplayName = displayName;
				needUpdating = true;
			}

			if (!user.Initials.Equals(initials))
			{
				user.Initials = initials;
				needUpdating = true;
			}

			if (needUpdating)
			{
				await _userTbl.Update(user);
			}
		}
		else
		{
			user = new UserTbl
			{
				Sub = userId,
				Role = UserRoles.Standard,
				Email = email,
				GivenName = givenName,
				FamilyName = familyName,
				DisplayName = displayName,
				Initials = initials
			};

			// If no admin users exist, make this user an admin
			if (!_userTbl.Where(x => x.Role.Equals(UserRoles.Admin)).Any())
			{
				user.Role = UserRoles.Admin;
			}

			user = await _userTbl.Add(user);
		}

		context.Principal.AddIdentity(new ClaimsIdentity(new Claim[]
		{
			new Claim(ClaimTypes.Role, user.Role.ToString()),
			new Claim("DisplayName", user.DisplayName),
			new Claim("Initials", user.Initials)
		}));
	}

	static string GetInstials(string? givenName, string? familyName, string email)
	{
		string initials = string.Empty;

		if (givenName is not null)
		{
			initials = givenName[0].ToString();
		}

		if (familyName is not null)
		{
			initials += familyName[0];
		}

		if (string.IsNullOrWhiteSpace(initials))
		{
			return email[0].ToString();
		}

		return initials;
	}
}