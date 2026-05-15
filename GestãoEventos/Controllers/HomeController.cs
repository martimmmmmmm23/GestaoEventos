using GestãoEventos.Data;
using GestãoEventos.Models;
using GestãoEventos.ViewModel.Eventos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GestãoEventos.Controllers
{
    public class HomeController : Controller
    {
        private readonly GestaoEventosDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(GestaoEventosDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var eventosDaDb = await _context.Eventos.ToListAsync();
            var model = eventosDaDb.Select(e => new EventoViewModel
            {
                Nome = e.Nome,
                Data = e.Data,
                Local = e.Local

            }).ToList();

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
