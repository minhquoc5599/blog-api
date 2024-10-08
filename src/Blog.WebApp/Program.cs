using Blog.Core.Configs;
using Blog.Core.Domain.Identity;
using Blog.Core.Models.Content;
using Blog.Core.SeedWorks;
using Blog.Data;
using Blog.Data.Repositories;
using Blog.Data.SeedWorks;
using Blog.WebApp.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection");
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
	.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

// Config DBContext and ASP.NET Core Identity
builder.Services.AddDbContext<BlogContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddIdentity<AppUser, AppRole>(options => options.SignIn.RequireConfirmedAccount = false)
	.AddEntityFrameworkStores<BlogContext>()
	.AddDefaultTokenProviders();



builder.Services.Configure<IdentityOptions>(options =>
{
	// Password settings
	options.Password.RequireDigit = true;
	options.Password.RequireLowercase = true;
	options.Password.RequireNonAlphanumeric = true;
	options.Password.RequireUppercase = true;
	options.Password.RequiredLength = 6;
	options.Password.RequiredUniqueChars = 1;

	// Lockout settings
	options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
	options.Lockout.MaxFailedAccessAttempts = 5;
	options.Lockout.AllowedForNewUsers = false;

	// User settings
	options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+/ ";
	options.User.RequireUniqueEmail = true;

});

// Add services to the container.
builder.Services.AddScoped(typeof(IRepository<,>), typeof(RepositoryBase<,>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Business services and repositories
var services = typeof(PostRepository).Assembly.GetTypes()
	.Where(x => x.GetInterfaces().Any(i => i.Name == typeof(IRepository<,>).Name)
	&& !x.IsAbstract && x.IsClass && !x.IsGenericType);

foreach (Type? service in services)
{
	var allInterfaces = service.GetInterfaces();
	var directInterface = allInterfaces.Except(allInterfaces.SelectMany(t => t.GetInterfaces())).FirstOrDefault();
	if (directInterface != null)
	{
		builder.Services.Add(new ServiceDescriptor(directInterface, service, ServiceLifetime.Scoped));
	}
}

builder.Services.AddAutoMapper(typeof(PostResponse));

// Auth
builder.Services.AddScoped<SignInManager<AppUser>, SignInManager<AppUser>>();
builder.Services.AddScoped<UserManager<AppUser>, UserManager<AppUser>>();
builder.Services.AddScoped<RoleManager<AppRole>, RoleManager<AppRole>>();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, CustomClaimsPrincipalFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
