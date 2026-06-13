using LabubuLog.Data;
using LabubuLog.Models;
using LabubuLog.Services.GameMetadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
}

// Add services to the container.
var databaseProvider = ResolveDatabaseProvider(builder.Configuration);
var connectionString = ResolveConnectionString(builder.Configuration, databaseProvider);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (IsSqlServer(databaseProvider))
    {
        options.UseSqlServer(connectionString);
        return;
    }

    if (IsSqlite(databaseProvider))
    {
        options.UseSqlite(connectionString);
        return;
    }

    throw new InvalidOperationException($"Unsupported database provider '{databaseProvider}'. Use 'Sqlite' or 'SqlServer'.");
});
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
});
builder.Services.AddScoped<IdentitySeedService>();
builder.Services.AddHttpClient<SteamGameMetadataProvider>();
builder.Services.AddHttpClient<RawgGameMetadataProvider>();
builder.Services.AddScoped<IGameMetadataProvider>(serviceProvider =>
    new CompositeGameMetadataProvider([
        serviceProvider.GetRequiredService<SteamGameMetadataProvider>(),
        serviceProvider.GetRequiredService<RawgGameMetadataProvider>()
    ]));
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Error");
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (dbContext.Database.IsSqlite())
    {
        var connection = dbContext.Database.GetDbConnection();

        if (!string.IsNullOrWhiteSpace(connection.DataSource))
        {
            var databaseDirectory = Path.GetDirectoryName(Path.GetFullPath(connection.DataSource));

            if (!string.IsNullOrWhiteSpace(databaseDirectory))
            {
                Directory.CreateDirectory(databaseDirectory);
            }
        }

        dbContext.Database.Migrate();
    }
    else
    {
        dbContext.Database.EnsureCreated();
    }

    var identitySeeder = scope.ServiceProvider.GetRequiredService<IdentitySeedService>();
    await identitySeeder.SeedAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

static string ResolveDatabaseProvider(IConfiguration configuration)
{
    var configuredProvider = configuration["DatabaseProvider"];

    if (!string.IsNullOrWhiteSpace(configuredProvider))
    {
        return configuredProvider.Trim();
    }

    var connectionString = configuration.GetConnectionString("LabubuLog");

    if (LooksLikeSqlServer(connectionString))
    {
        return "SqlServer";
    }

    return "Sqlite";
}

static string ResolveConnectionString(IConfiguration configuration, string databaseProvider)
{
    var connectionString = configuration.GetConnectionString("LabubuLog");

    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        return connectionString;
    }

    if (IsSqlServer(databaseProvider))
    {
        throw new InvalidOperationException("Missing SQL Server connection string. Set ConnectionStrings__LabubuLog in the hosting environment.");
    }

    return "Data Source=Data/labubulog.db";
}

static bool LooksLikeSqlServer(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return false;
    }

    return connectionString.Contains("Initial Catalog=", StringComparison.OrdinalIgnoreCase)
        || connectionString.Contains("User ID=", StringComparison.OrdinalIgnoreCase)
        || connectionString.Contains("User Id=", StringComparison.OrdinalIgnoreCase)
        || connectionString.Contains("Trusted_Connection=", StringComparison.OrdinalIgnoreCase)
        || connectionString.Contains("TrustServerCertificate=", StringComparison.OrdinalIgnoreCase);
}

static bool IsSqlite(string databaseProvider) =>
    string.Equals(databaseProvider, "Sqlite", StringComparison.OrdinalIgnoreCase);

static bool IsSqlServer(string databaseProvider) =>
    string.Equals(databaseProvider, "SqlServer", StringComparison.OrdinalIgnoreCase);
