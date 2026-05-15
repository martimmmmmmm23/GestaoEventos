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

            // ADICIONADO: Precisamos do Contexto para gravar na tabela Participantes
            var context = service.GetRequiredService<GestaoEventosDbContext>();

            // Criar Roles se não existirem
            await roleManager.CreateAsync(new IdentityRole("Organizador"));
            await roleManager.CreateAsync(new IdentityRole("Utilizador"));

            // 2. CRIAR O ADMIN 
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
            // 3. CRIAR CONTAS DE TESTE (PARTICIPANTES)
            // Utilizador: João
            if (await userManager.FindByEmailAsync("joao@participante.pt") == null)
            {
                var joao = new ApplicationUser
                {
                    UserName = "joao@participante.pt",
                    Email = "joao@participante.pt",
                    NomeCompleto = "João Silva",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(joao, "Teste123!");
                if (result.Succeeded)
                {
                    context.Participantes.Add(new Participante { Nome = joao.NomeCompleto, Email = joao.Email });
                }
            }

            // Utilizador: Maria
            if (await userManager.FindByEmailAsync("maria@participante.pt") == null)
            {
                var maria = new ApplicationUser
                {
                    UserName = "maria@participante.pt",
                    Email = "maria@participante.pt",
                    NomeCompleto = "Maria Santos",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(maria, "Teste123!");
                if (result.Succeeded)
                {
                    context.Participantes.Add(new Participante { Nome = maria.NomeCompleto, Email = maria.Email });
                }
            }

            // GUARDA TUDO NA BASE DE DADOS
            await context.SaveChangesAsync();
        }
    }
}
