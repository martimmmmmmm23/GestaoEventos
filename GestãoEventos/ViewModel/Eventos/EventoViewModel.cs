using System.ComponentModel.DataAnnotations;

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

        [Display(Name = "Descrição")]
        [StringLength(500)]
        public string? Descricao { get; set; }

        [Display(Name = "Detalhes")]
        public string? Detalhes { get; set; }
    }
}