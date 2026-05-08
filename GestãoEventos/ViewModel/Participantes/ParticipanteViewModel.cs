using System.ComponentModel.DataAnnotations;

namespace GestãoEventos.ViewModel.Participantes
{
    public class ParticipanteViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
        [Display(Name = "Nome do Participante")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Insira um endereço de email válido.")]
        [Display(Name = "Email Principal")]
        public string Email { get; set; }

        // Example of an extra property often needed in UIs but not in the Database
        [Display(Name = "Confirmar Email")]
        [Compare("Email", ErrorMessage = "Os emails não coincidem.")]
        public string ConfirmarEmail { get; set; }
    }
}
