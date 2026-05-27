using GestãoEventos.Data.Classes;
using GestãoEventos.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GestãoEventos.Data
{
    public class GestaoEventosDbContext : IdentityDbContext<ApplicationUser>
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

            modelBuilder.Entity<Inscricao>()
                .HasOne(i => i.Evento)
                .WithMany(e => e.Inscricoes)
                .HasForeignKey(i => i.EventoId)
                .OnDelete(DeleteBehavior.Cascade); // Se apagar o Evento, apaga as Inscrições dele

            modelBuilder.Entity<Inscricao>()
                .HasOne(i => i.Participante)
                .WithMany(p => p.Inscricoes)
                .HasForeignKey(i => i.ParticipanteId)
                .OnDelete(DeleteBehavior.Cascade); // Se apagar o Participante, apaga as Inscrições dele
        }
    }
}
