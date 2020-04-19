using MathEvent.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Initializers
{
    public static class RolesInitializer
    {
        public static async Task Initialize(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var applicationRoles = new List<string> { "admin", "user" };

            foreach (var role in applicationRoles)
            {
                if (await roleManager.FindByNameAsync(role) == null)
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            await InitializeAdminAccount(userManager);
        }

        private static async Task InitializeAdminAccount(UserManager<ApplicationUser> userManager)
        {
            const string adminEmail = "admin@admin.ru";
            const string adminPassword = "!1Qwerty";

            if (await userManager.FindByNameAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser { Name = "Админ", Surname = "Админ", Email = adminEmail, UserName = adminEmail };
                var result = await userManager.CreateAsync(admin, adminPassword);

                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "admin");
            }
        }
    }
}
