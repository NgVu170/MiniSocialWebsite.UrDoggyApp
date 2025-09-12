using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using UrDoggy.Core.Models;

namespace UrDoggy.Website
{
    public class AdminSeeder
    {
        public static async Task SeederAdmin(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<AdminSeeder>>();

                try
                {
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
                    var userManager = services.GetRequiredService<UserManager<User>>();

                    logger.LogInformation("Starting admin seeding...");

                    // Tạo roles nếu chưa tồn tại
                    string[] roleNames = { "Admin", "Customer" };
                    foreach (var roleName in roleNames)
                    {
                        if (!await roleManager.RoleExistsAsync(roleName))
                        {
                            await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                            logger.LogInformation($"Created role: {roleName}");
                        }
                    }

                    // Tạo admin user nếu chưa tồn tại
                    var adminUser = await userManager.FindByNameAsync("admin");
                    if (adminUser == null)
                    {
                        var user = new User
                        {
                            UserName = "admin",
                            Email = "admin@gmail.com",
                            EmailConfirmed = true,
                            DisplayName = "Administrator",
                            ProfilePicture = "/images/default-avatar.png",
                            Bio = "System Administrator",
                            IsAdmin = true
                        };

                        string userPwd = "admin123";
                        var createResult = await userManager.CreateAsync(user, userPwd);

                        if (createResult.Succeeded)
                        {
                            await userManager.AddToRoleAsync(user, "Admin");
                            logger.LogInformation("Admin user created successfully");
                        }
                        else
                        {
                            var errorMessages = string.Join("; ", createResult.Errors.Select(e => e.Description));
                            logger.LogError($"Admin user creation failed: {errorMessages}");
                            throw new Exception($"Admin user creation failed: {errorMessages}");
                        }
                    }
                    else
                    {
                        logger.LogInformation("Admin user already exists");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while seeding admin user");
                    throw;
                }
            }
        }
    }
}