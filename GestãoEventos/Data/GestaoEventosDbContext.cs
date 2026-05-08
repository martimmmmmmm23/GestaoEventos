using Microsoft.EntityFrameworkCore;
using GestãoEventos.Data.Classes;
using GestãoEventos.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace GestãoEventos.Data
{
    public class GestaoEventosDbContext : IdentityDbContext<ApplicationUser>
    {
        public GestaoEventosDbContext(DbContextOptions<GestaoEventosDbContext> options)
     : base(options)
        {
        }
        public DbSet<Evento> Eventos { get; set; }

        public DbSet<Inscricao> Inscricoes { get; set; }

        public DbSet<Participante> Participantes { get; set; } = default!;
    }
}
