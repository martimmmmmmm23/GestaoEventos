using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestãoEventos.Controllers
{
    // Controlador para funcionalidades administrativas
    [Authorize(Roles = "Organizador")] // Apenas usuários com a função "Organizador" podem acessar
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
