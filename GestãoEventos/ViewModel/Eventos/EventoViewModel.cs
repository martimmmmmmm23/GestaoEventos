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
    }
}
