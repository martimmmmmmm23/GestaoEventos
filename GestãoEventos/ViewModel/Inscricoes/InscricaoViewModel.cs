using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestãoEventos.ViewModel.Inscricoes
{
    public class InscricaoViewModel
    {
        public int? ParticipanteId { get; set; }

        //[Required(ErrorMessage = "O nome é obrigatório.")]
        [Display(Name = "Nome Completo")]
        public string? Nome { get; set; }

        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "O email deve ser válido.")] // Validação de email em relacao a um padrao ex: abc@domain.com
        [Display(Name = "Endereço de Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A seleção do evento é obrigatória.")]
        [Display(Name = "Evento")]
        public int EventoId { get; set; }

        public string? NomeEvento { get; set; }

        public SelectList? EventosDisponiveis { get; set; } // SelectList facilita a criação de dropdowns no Razor, permitindo ver a lista de opções para o utilizador escolher.


    }
}
