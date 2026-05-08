using Microsoft.EntityFrameworkCore;
using GestãoEventos.Data.Classes;

namespace GestãoEventos.Data
{
    public class GestaoEventosDbContext : DbContext
    {
        public GestaoEventosDbContext(DbContextOptions<GestaoEventosDbContext> options)
     : base(options)
        {
        }
        public DbSet<GestãoEventos.Data.Classes.Participante> Participante { get; set; } = default!;
    }
}
