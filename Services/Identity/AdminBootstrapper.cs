using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RentWisePro.Web.Models.Identity;

namespace RentWisePro.Web.Services.Identity;

public static class AdminBootstrapper
{
    public static async Task EnsureAdminAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AdminBootstrapper");
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (!await roleManager.RoleExistsAsync(RoleNames.Admin))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(RoleNames.Admin));
            if (!roleResult.Succeeded)
            {
                logger.LogWarning("Failed to create Admin role: {Errors}", string.Join(", ", roleResult.Errors.Select(error => error.Description)));
                return;
            }
        }

        var enabled = configuration.GetValue<bool>("AdminBootstrap:Enabled");
        if (!enabled)
        {
            return;
        }

        var email = configuration["AdminBootstrap:Email"];
        if (string.IsNullOrWhiteSpace(email))
        {
            logger.LogWarning("Admin bootstrap is enabled but AdminBootstrap:Email is not set.");
            return;
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            var password = configuration["AdminBootstrap:Password"];
            if (string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning("Admin bootstrap could not create {Email} because AdminBootstrap:Password is not set.", email);
                return;
            }

            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                logger.LogWarning("Failed to create admin user {Email}: {Errors}", email, string.Join(", ", createResult.Errors.Select(error => error.Description)));
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(user, RoleNames.Admin))
        {
            var addResult = await userManager.AddToRoleAsync(user, RoleNames.Admin);
            if (!addResult.Succeeded)
            {
                logger.LogWarning("Failed to add admin role to {Email}: {Errors}", email, string.Join(", ", addResult.Errors.Select(error => error.Description)));
            }
        }
    }
}
