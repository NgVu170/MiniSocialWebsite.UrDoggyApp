using Microsoft.AspNetCore.Identity;
using UrDoggy.Core.Models;

namespace UrDoggy.Website
{
    public static class AdminSeeder
    {
        public static async Task SeederAdmin(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

            string adminEmail = "admin@urdoggy.local";
            string adminPassword = "Admin123!";

            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole<int>("Admin"));

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new User
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true,
                    // thêm 3 dòng này:
                    DisplayName = "Admin",
                    Bio = string.Empty,
                    ProfilePicture = string.Empty
                };
                await userManager.CreateAsync(admin, adminPassword);
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
