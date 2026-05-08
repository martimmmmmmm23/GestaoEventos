using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestãoEventos.Controllers
{
    // Controlador para funcionalidades administrativas
    [Authorize(Roles = "Admin")] // Apenas usuários com a função "Admin" podem acessar
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
