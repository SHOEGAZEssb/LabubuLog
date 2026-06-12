using LabubuLog.Data;
using LabubuLog.Services.GameMetadata;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("LabubuLog")
    ?? "Data Source=Data/labubulog.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddHttpClient<IGameMetadataProvider, SteamGameMetadataProvider>();
builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
