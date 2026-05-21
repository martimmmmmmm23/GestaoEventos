using GestãoEventos.Data.Classes;
using System.ComponentModel.DataAnnotations;

namespace GestãoEventos.ViewModel.Participantes
{
    public class PerfilViewModel
    {
        public int Id { get; set; }

        // --- DADOS PARA MOSTRAR NO ECRÃ ---
        public string? NomeAtual { get; set; }
        public string? EmailAtual { get; set; }

        // --- CAMPOS PARA ALTERAR ---
        [Display(Name = "Novo Nome Completo")]
        public string? NovoNome { get; set; }

        [EmailAddress(ErrorMessage = "Email inválido.")]
        [Display(Name = "Novo Email")]
        public string? NovoEmail { get; set; }

        [Display(Name = "Confirmar Novo Email")]
        [Compare("NovoEmail", ErrorMessage = "Os emails inseridos não coincidem.")]
        public string? ConfirmarNovoEmail { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Palavra-passe Atual")]
        public string? PasswordAtual { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Nova Palavra-passe")]
        public string? NovaPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Nova Palavra-passe")]
        [Compare("NovaPassword", ErrorMessage = "As novas palavras-passe não coincidem.")]
        public string? ConfirmarNovaPassword { get; set; }

        public ICollection<Inscricao> Inscricoes { get; set; } = new List<Inscricao>();
    }
}