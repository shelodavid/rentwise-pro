using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RentWisePro.Web.Models.Identity;

namespace RentWisePro.Web.Services;

public static class AdminBootstrapper
{
    public static async Task RunAsync(
        IServiceProvider services,
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger logger)
    {
        if (!environment.IsDevelopment())
        {
            return;
        }

        var enabled = configuration.GetValue<bool>("AdminBootstrap:Enabled");
        if (!enabled)
        {
            logger.LogInformation("Admin bootstrap is disabled.");
            return;
        }

        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (!await roleManager.RoleExistsAsync(RoleNames.Admin))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(RoleNames.Admin));
            if (!roleResult.Succeeded)
            {
                logger.LogWarning(
                    "Failed to create Admin role: {Errors}",
                    string.Join(", ", roleResult.Errors.Select(error => error.Description)));
                return;
            }

            logger.LogInformation("Created Admin role.");
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
                logger.LogWarning(
                    "Admin bootstrap could not create {Email} because AdminBootstrap:Password is not set.",
                    email);
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
                logger.LogWarning(
                    "Failed to create admin user {Email}: {Errors}",
                    email,
                    string.Join(", ", createResult.Errors.Select(error => error.Description)));
                return;
            }

            logger.LogInformation("Created admin user {Email}.", email);
        }
        else
        {
            logger.LogInformation("Admin bootstrap found existing user {Email}.", email);
        }

        if (!await userManager.IsInRoleAsync(user, RoleNames.Admin))
        {
            var addResult = await userManager.AddToRoleAsync(user, RoleNames.Admin);
            if (!addResult.Succeeded)
            {
                logger.LogWarning(
                    "Failed to add admin role to {Email}: {Errors}",
                    email,
                    string.Join(", ", addResult.Errors.Select(error => error.Description)));
                return;
            }

            logger.LogInformation("Added Admin role to {Email}.", email);
        }
        else
        {
            logger.LogInformation("Admin user {Email} already has Admin role.", email);
        }
    }
}
