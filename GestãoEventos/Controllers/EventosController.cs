using GestãoEventos.Data;
using GestãoEventos.Data.Classes;
using GestãoEventos.ViewModel.Eventos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        [Authorize(Roles = "Organizador")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Eventos.ToListAsync());
        }

        // GET: Eventos/Details/5
        [Authorize(Roles = "Organizador")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var evento = await _context.Eventos
                        .Include(e => e.Inscricoes)
                        .ThenInclude(e => e.Participante)
                        .FirstOrDefaultAsync(x => x.Id == id);

            if (evento == null)
                return NotFound();

            ViewBag.EventoId = evento.Id;

            var model = new EventoViewModel
            {
                Nome = evento.Nome,
                Data = evento.Data,
                Local = evento.Local,
                Image = evento.Image,
                Descricao = evento.Descricao,
                Hora = evento.Hora,
                Preco = evento.Preco,
                Inscricoes = evento.Inscricoes,
                Lugares = evento.Lugares
            };

            return View(model);
        }

        // GET: Eventos/Create
        [Authorize(Roles = "Organizador")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Eventos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Organizador")]
        public async Task<IActionResult> Create(EventoViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            bool localOcupado = await _context.Eventos.AnyAsync(e =>
                e.Data == model.Data &&
                e.Hora == model.Hora &&
                e.Local == model.Local);

            if (localOcupado)
            {
                ModelState.AddModelError(string.Empty, "Conflito de agenda: Já existe um evento marcado para este Local, nessa Data e Hora.");
                return View(model);
            }

            var evento = new Evento
            {
                Nome = model.Nome,
                Data = model.Data,
                Local = model.Local,
                Descricao = model.Descricao,
                Hora = model.Hora,
                Preco = model.Preco,
                Lugares = model.Lugares
            };

            if (model.ImageFile != null)
            {
                if (model.ImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "A imagem não pode exceder 5MB.");
                    return View(model);
                }

                var fileName = Guid.NewGuid().ToString() +
                               Path.GetExtension(model.ImageFile.FileName);

                var path = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/images",
                    fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                evento.Image = "/images/" + fileName;
            }

            _context.Add(evento);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Eventos/Edit/5
        [Authorize(Roles = "Organizador")]
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
                Local = evento.Local,
                Image = evento.Image,
                Descricao = evento.Descricao,
                Hora = evento.Hora,
                Preco = evento.Preco,
                Lugares = evento.Lugares
            };

            ViewBag.Id = id;

            return View(model);
        }

        // POST: Eventos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Organizador")]
        public async Task<IActionResult> Edit(int id, EventoViewModel model)
        {
          
            var evento = await _context.Eventos.FindAsync(id);

            if (evento == null)
                return NotFound();

            bool localOcupado = await _context.Eventos.AnyAsync(e =>
              e.Id != id &&
              e.Data == model.Data &&
              e.Hora == model.Hora &&
              e.Local == model.Local);

            if (localOcupado)
            {
                ModelState.AddModelError(string.Empty, "Conflito de agenda: Já existe outro evento marcado para este Local, nessa Data e Hora.");
                ViewBag.Id = id;
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Id = id;
                return View(model);
            }

            evento.Nome = model.Nome;
            evento.Data = model.Data;
            evento.Local = model.Local;
            evento.Descricao = model.Descricao;
            evento.Hora = model.Hora;
            evento.Preco = model.Preco;
            evento.Lugares = model.Lugares;

            if (model.ImageFile != null)
            {
                if (model.ImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "A imagem não pode exceder 5MB.");
                    ViewBag.Id = id;
                    return View(model);
                }

                var fileName = Guid.NewGuid().ToString() +
                               Path.GetExtension(model.ImageFile.FileName);

                var path = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/images/",
                    fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                evento.Image = "/images/" + fileName;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Eventos/Delete/5
        [Authorize(Roles = "Organizador")]
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
        [Authorize(Roles = "Organizador")]
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

        public async Task<IActionResult> ExportParticipants(int id)
        {
            var evento = await _context.Eventos
                .Include(e => e.Inscricoes)
                    .ThenInclude(i => i.Participante)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evento == null)
                return NotFound();

            var csv = new StringBuilder();

       
            csv.AppendLine($"Evento: {evento.Nome}");
            csv.AppendLine($"Data: {evento.Data:dd/MM/yyyy}");
            csv.AppendLine("");

           
            csv.AppendLine("Nome,Email");

           
            foreach (var insc in evento.Inscricoes)
            {
                csv.AppendLine($"{insc.Participante.Nome},{insc.Participante.Email}");
            }

            var fileName = $"evento_{SanitizeFileName(evento.Nome)}_participantes.csv";

            return File(
                Encoding.UTF8.GetBytes(csv.ToString()),
                "text/csv",
                fileName
            );
        }

        private string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "evento";

            var invalidChars = Path.GetInvalidFileNameChars();

            foreach (var c in invalidChars)
            {
                name = name.Replace(c, '_');
            }

            return name.Replace(" ", "_");
        }
    }
}