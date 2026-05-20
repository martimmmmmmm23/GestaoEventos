using GestãoEventos.Data;
using GestãoEventos.Data.Classes;
using GestãoEventos.Models;
using GestãoEventos.ViewModel.Participantes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GestãoEventos.Controllers
{
    public class ParticipantesController : Controller
    {
        private readonly GestaoEventosDbContext _context;

        //para gerir os users do ASP.NET Core Identity
        private readonly UserManager<ApplicationUser> _userManager;
        public ParticipantesController(
                    GestaoEventosDbContext context,
                    UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
            return RedirectToPage("/Account/Register", new { area = "Identity" });
        }


        // GET: Participantes/Edit/5
        [Authorize(Roles = "Organizador")]
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
        [Authorize(Roles = "Organizador")]
        public async Task<IActionResult> Edit(int id, ParticipanteViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Buscar participante
            var participante = await _context.Participantes.FindAsync(id);

            if (participante == null)
                return NotFound();

            // Guardar email antigo (IMPORTANTE)
            var oldEmail = participante.Email;

            // Atualizar tabela Participantes
            participante.Nome = model.Nome;
            participante.Email = model.Email;

            // Procurar user no AspNetUsers
            var user = await _userManager.FindByEmailAsync(oldEmail);

            if (user != null)
            {
                // Atualizar email e username
                user.Email = model.Email;
                user.UserName = model.Email;

                // Atualizar nome (AspNetUsers)
                user.NomeCompleto = model.Nome;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "Utilizador do AspNetUsers não encontrado.");
                return View(model);
            }

            // Guardar alterações na tabela Participantes
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Participantes/Delete/5
        [Authorize(Roles = "Organizador")]
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
        [Authorize(Roles = "Organizador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Procurar participante
            var participante = await _context.Participantes
                .FirstOrDefaultAsync(p => p.Id == id);

            if (participante == null)
            {
                return NotFound();
            }

            // Procurar utilizador no AspNetUsers pelo email
            var user = await _userManager.FindByEmailAsync(participante.Email);

            // Apagar participante
            _context.Participantes.Remove(participante);

            // Apagar utilizador Identity
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
