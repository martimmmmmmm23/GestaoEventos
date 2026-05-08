using Microsoft.AspNetCore.Identity;

namespace GestãoEventos.Models
{
    // Herdar IdentityUser para usar o que já existe (Email, Password, etc)
    // Adicionar propriedades personalizadas (NomeCompleto)
    public class ApplicationUser : IdentityUser
    {
        public string NomeCompleto { get; set; }
    }
}
