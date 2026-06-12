using LabubuLog.Models;
using Microsoft.AspNetCore.Identity;

namespace LabubuLog.Data;

public class IdentitySeedService(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    IWebHostEnvironment environment)
{
    public async Task SeedAsync()
    {
        await EnsureUserAsync("lebubu", "Lebubu", "Lebubu Score", "SeedUsers:LebubuPassword", "Lebubu123!");
        await EnsureUserAsync("labubu", "Labubu", "Labubu Score", "SeedUsers:LabubuPassword", "Labubu123!");
    }

    private async Task EnsureUserAsync(
        string userName,
        string displayName,
        string scoreLabel,
        string passwordKey,
        string developmentPassword)
    {
        var user = await userManager.FindByNameAsync(userName);

        if (user is not null)
        {
            user.DisplayName = displayName;
            user.ScoreLabel = scoreLabel;
            await userManager.UpdateAsync(user);
            return;
        }

        var password = configuration[passwordKey];

        if (string.IsNullOrWhiteSpace(password))
        {
            if (!environment.IsDevelopment())
            {
                throw new InvalidOperationException($"Missing required seed password configuration: {passwordKey}");
            }

            password = developmentPassword;
        }

        user = new ApplicationUser
        {
            UserName = userName,
            DisplayName = displayName,
            ScoreLabel = scoreLabel
        };

        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Could not seed user '{userName}': {errors}");
        }
    }
}
