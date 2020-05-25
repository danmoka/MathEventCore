using MathEvent.Helpers;
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
        }
    }
}
