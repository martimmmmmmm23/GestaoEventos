using Microsoft.AspNetCore.Identity;
using GestãoEventos.Models;

namespace GestãoEventos.Data
{
    public class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            // Gestores de Utilizadores e Roles
            var userManager = service.GetService<UserManager<ApplicationUser>>();
            var roleManager = service.GetService<RoleManager<IdentityRole>>();

            // Criar Roles se não existirem
            await roleManager.CreateAsync(new IdentityRole("Organizador"));
            await roleManager.CreateAsync(new IdentityRole("Utilizador"));

            // Criar um Admin se não existir
            var adminEmail = "admin@portal.pt";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    NomeCompleto = "Administrador do Sistema",
                    EmailConfirmed = true
                };

                // Criar o Admin com uma password forte
                await userManager.CreateAsync(adminUser, "Admin123!");

                // Atribuir Role Admin
                await userManager.AddToRoleAsync(adminUser, "Organizador");
            }
        }
    }
}
