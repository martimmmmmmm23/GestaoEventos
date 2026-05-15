using System.ComponentModel.DataAnnotations;
using GestãoEventos.Data.Classes;

namespace GestãoEventos.ViewModel.Eventos
{
    public class EventoViewModel
    {
        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Data { get; set; }

        [Required]
        public string Local { get; set; }

        [Display(Name = "Imagem")]
        public string? Image { get; set; }

        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Descrição")]
        [StringLength(500)]
        public string? Descricao { get; set; }

        [Display(Name = "Detalhes")]
        public string? Detalhes { get; set; }

        public ICollection<Inscricao> Inscricoes { get; set; } = new List<Inscricao>();
    }
}