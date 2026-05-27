using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

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

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Descrição")]
        [StringLength(500)]
        public string? Descricao { get; set; }

        [Display(Name = "Hora")]
        [Required]
        public TimeSpan Hora { get; set; }

        [Display(Name = "Preço")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? Preco { get; set; }

        [Display(Name = "Total de Lugares")]
        public int? Lugares { get; set; } // null indica que não há limite de lugares

        public ICollection<Inscricao> Inscricoes { get; set; } = new List<Inscricao>();
    }
}