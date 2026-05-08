using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestãoEventos.Data;
using GestãoEventos.Data.Classes;
using GestãoEventos.ViewModel.Eventos;

namespace GestãoEventos.Controllers
{
    public class EventosController : Controller
    {
        private readonly GestaoEventosDbContext _context;

        public EventosController(GestaoEventosDbContext context)
        {
            _context = context;
        }

        // GET: Eventos
        public async Task<IActionResult> Index()
        {
            return View(await _context.Eventos.ToListAsync());
        }

        // GET: Eventos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var evento = await _context.Eventos
                .FirstOrDefaultAsync(x => x.Id == id);

            if (evento == null)
                return NotFound();

            var model = new EventoViewModel
            {
                Nome = evento.Nome,
                Data = evento.Data,
                Local = evento.Local
            };

            return View(model);
        }

        // GET: Eventos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Eventos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventoViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var evento = new Evento
            {
                Nome = model.Nome,
                Data = model.Data,
                Local = model.Local
            };

            _context.Add(evento);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Eventos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var evento = await _context.Eventos.FindAsync(id);

            if (evento == null)
                return NotFound();

            var model = new EventoViewModel
            {
                Nome = evento.Nome,
                Data = evento.Data,
                Local = evento.Local
            };

            ViewBag.Id = id; // porque não usas Id na ViewModel

            return View(model);
        }

        // POST: Eventos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventoViewModel model)
        {
            var evento = await _context.Eventos.FindAsync(id);

            if (evento == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Id = id;
                return View(model);
            }

            evento.Nome = model.Nome;
            evento.Data = model.Data;
            evento.Local = model.Local;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Eventos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var evento = await _context.Eventos
                .FirstOrDefaultAsync(x => x.Id == id);

            if (evento == null)
                return NotFound();

            return View(evento);
        }

        // POST: Eventos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var evento = await _context.Eventos.FindAsync(id);

            if (evento != null)
            {
                _context.Eventos.Remove(evento);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
