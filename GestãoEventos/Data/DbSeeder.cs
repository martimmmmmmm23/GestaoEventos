using GestãoEventos.Data.Classes;
using GestãoEventos.Models;
using Microsoft.AspNetCore.Identity;

namespace GestãoEventos.Data
{
    public class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            // Gestores de Utilizadores e Roles
            var userManager = service.GetService<UserManager<ApplicationUser>>();
            var roleManager = service.GetService<RoleManager<IdentityRole>>();

            var context = service.GetRequiredService<GestaoEventosDbContext>();

            // Criar Roles se não existirem
            await roleManager.CreateAsync(new IdentityRole("Organizador"));
            await roleManager.CreateAsync(new IdentityRole("Utilizador"));

            // CRIAR O ADMIN 
            if (await userManager.FindByEmailAsync("admin@portal.pt") == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@portal.pt",
                    Email = "admin@portal.pt",
                    NomeCompleto = "Administrador do Sistema",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Organizador");

                    // GUARDAR NA TABELA PARTICIPANTES
                    context.Participantes.Add(new Participante { Nome = admin.NomeCompleto, Email = admin.Email });
                }
            }

            // GUARDA TUDO NA BASE DE DADOS
            await context.SaveChangesAsync();
        }
    }
}
