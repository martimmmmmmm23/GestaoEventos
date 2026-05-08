using GestãoEventos.Data.Classes;
using Microsoft.EntityFrameworkCore;

namespace GestãoEventos.Data
{
    public class GestaoEventosDbContext : DbContext
    {
        public GestaoEventosDbContext(DbContextOptions<GestaoEventosDbContext> options)
     : base(options)
        {
        }

        public DbSet<Evento> Eventos { get; set; }
        public DbSet<Participante> Participantes { get; set; }
        public DbSet<Inscricao> Inscricoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Inscricao>()
                .HasKey(i => new { i.EventoId, i.ParticipanteId });
        }

        ///--- CONFIGURAÇÃO DA RELAÇÃO MUITOS-PARA-MUITOS (M:N) ---
        // 1. Definimos 'EventoId' e 'ParticipanteId' como uma Chave Primária Composta
        // 2. O EF Core identifica automaticamente as Chaves Estrangeiras (FKs) porque:
        //    - A classe 'Inscricao' tem as propriedades de objeto e ID de Evento e Participante
        //    - As classes 'Evento' e 'Participante' têm listas (ICollection) de 'Inscricao'
        // 3. Conclusão do EFcore: Isto é claramente uma tabela de ligação (Muitos-para-Muitos).
        // Vou criar as chaves estrangeiras (Foreign Keys) na base de dados automaticamente!"

    }
}