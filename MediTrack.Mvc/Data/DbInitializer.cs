using MediTrack.Mvc.Models;
using Microsoft.AspNetCore.Identity;

namespace MediTrack.Mvc.Data;

public static class DbInitializer
{
    public static async Task SeedIdentityAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = { "Admin", "Staff", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        await CreateUser(userManager, "admin@shop.test", "Admin@123", "Admin", "Quản trị viên");
        await CreateUser(userManager, "staff@shop.test", "Staff@123", "Staff", "Nhân viên kho");
        await CreateUser(userManager, "user@shop.test", "User@123", "User", "Người dùng demo");
    }

    private static async Task CreateUser(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string role,
        string fullName)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
