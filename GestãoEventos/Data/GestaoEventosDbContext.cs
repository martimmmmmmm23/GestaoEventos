using Microsoft.EntityFrameworkCore;

namespace GestãoEventos.Data
{
    public class GestaoEventosDbContext : DbContext
    {
        public GestaoEventosDbContext(DbContextOptions<GestaoEventosDbContext> options)
     : base(options)
        {
        }
    }
}
