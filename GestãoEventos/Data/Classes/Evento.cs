using System.ComponentModel.DataAnnotations;

namespace GestãoEventos.Data.Classes
{
    public class Evento
    {
        public int Id { get; set; }

        

        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [Display(Name = "Data")]
        [Required]
        public DateTime Data { get; set; }

        [Required]
        public string Local { get; set; }

        [Display(Name = "Imagem")]
        public string? Image { get; set; }

        [Display(Name = "Descrição")]
        [StringLength(500)]
        public string? Descricao { get; set; }

        [Display(Name = "Detalhes")]
        public string? Detalhes { get; set; }
        public ICollection<Inscricao> Inscricoes { get; set; } = new List<Inscricao>();
    }
}
