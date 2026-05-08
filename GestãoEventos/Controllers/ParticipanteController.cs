using GestãoEventos.Data;
using GestãoEventos.Data.Classes;
using GestãoEventos.ViewModel.Participantes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GestãoEventos.Controllers
{
    public class ParticipanteController : Controller
    {
        private readonly GestaoEventosDbContext _context;

        public ParticipanteController(GestaoEventosDbContext context)
        {
            _context = context;
        }

        // GET: Participantes
        public async Task<IActionResult> Index()
        {
            var listParticipante = _context.Participantes;

            return View(await listParticipante.ToListAsync());
        }

        // GET: Participantes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var participante = await _context.Participantes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (participante == null)
            {
                return NotFound();
            }
            ViewBag.SelectedId = id;
            return View(participante);
        }

        // GET: Participantes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Participantes/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ParticipanteViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Convert ViewModel -> Entity
                var participante = new Participante
                {
                    Nome = model.Nome,
                    Email = model.Email
                };

                _context.Add(participante);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Participantes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var participante = await _context.Participantes.FindAsync(id);
            if (participante == null) return NotFound();

            ViewBag.SelectedId = id;
            var viewModel = new ParticipanteViewModel
            {
                Nome = participante.Nome,
                Email = participante.Email,
                ConfirmarEmail = participante.Email // Pre-fill so validation doesn't fail immediately
            };

            return View(viewModel);
        }

            // POST: Participantes/Edit/5
            [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ParticipanteViewModel model)
        {
            if (!(_context.Participantes.Select(x => x.Id).Contains(id))) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var participante = await _context.Participantes.FindAsync(id);
                    if (participante == null) return NotFound();

                    participante.Nome = model.Nome;
                    participante.Email = model.Email;

                    _context.Update(participante);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Participantes.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Participantes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var participante = await _context.Participantes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (participante == null)
            {
                return NotFound();
            }
            ViewBag.SelectedId = id;
            return View(participante);
        }

        // POST: Participantes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var participante = await _context.Participantes.FindAsync(id);
            if (participante != null)
            {
                _context.Participantes.Remove(participante);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
