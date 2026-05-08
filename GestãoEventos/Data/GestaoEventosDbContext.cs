using Microsoft.EntityFrameworkCore;
using GestãoEventos.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using GestãoEventos.Data.Classes;

namespace GestãoEventos.Data
{
    public class GestaoEventosDbContext : IdentityDbContext<ApplicationUser>
    {
        public GestaoEventosDbContext(DbContextOptions<GestaoEventosDbContext> options)
     : base(options)
        {
        }
        DbSet<Evento> Eventos { get; set; }

        //DbSet<Inscricao> Inscricoes { get; set; }

        DbSet<Participante> Participantes { get; set; }
    }
}
