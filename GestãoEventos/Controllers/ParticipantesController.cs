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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ParticipantesController(GestaoEventosDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: Participantes
        [Authorize(Roles = "Organizador")]
        public async Task<IActionResult> Index()
        {
            var listParticipante = _context.Participantes;

            return View(await listParticipante.ToListAsync());
        }

        // GET: Participantes/Details/5
        [Authorize(Roles = "Organizador")]
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

        // GET: Participantes/HomePage/5
        [Authorize]
        public async Task<IActionResult> HomePage(int? id)
        {
            if (id == null) return NotFound();
            var participante = await _context.Participantes
                .Include(p => p.Inscricoes)
                .ThenInclude(i => i.Evento)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (participante == null) return NotFound();

            // Bloqueia se o utilizador tentar ver outro perfil
            if (participante.Email != User.Identity.Name)
            {
                return Forbid();
            }

            var model = new PerfilViewModel
            {
                Id = participante.Id,
                NomeAtual = participante.Nome,
                EmailAtual = participante.Email,
                NovoNome = participante.Nome,
                NovoEmail = participante.Email,
                ConfirmarNovoEmail = participante.Email,
                Inscricoes = participante.Inscricoes
            };

            return View(model);
        }

        // POST: Participantes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> HomePage(int id, PerfilViewModel model)
        {
            if (id != model.Id) return NotFound();

            var participante = await _context.Participantes.FindAsync(id);
            if (participante == null) return NotFound();

            if (!User.IsInRole("Organizador") && participante.Email != User.Identity.Name) return Forbid();

            var userLogin = await _userManager.FindByEmailAsync(participante.Email);

            if (ModelState.IsValid)
            {
                bool precisaAtualizarLogin = false;

                // Alterar Nome
                if (!string.IsNullOrEmpty(model.NovoNome))
                {
                    participante.Nome = model.NovoNome;
                    if (userLogin != null) userLogin.NomeCompleto = model.NovoNome;
                }

                // Alterar Email
                if (model.NovoEmail != participante.Email)
                {
                    if (userLogin != null)
                    {
                        await _userManager.SetEmailAsync(userLogin, model.NovoEmail);
                        await _userManager.SetUserNameAsync(userLogin, model.NovoEmail);
                        precisaAtualizarLogin = true;
                    }
                    participante.Email = model.NovoEmail;
                }

                // Alterar Password
                if (!string.IsNullOrEmpty(model.NovaPassword))
                {
                    if (string.IsNullOrEmpty(model.PasswordAtual))
                    {
                        ModelState.AddModelError("PasswordAtual", "Para alterar a password, precisa de inserir a password atual.");
                        return RepopularView(model, participante);
                    }

                    var resultPass = await _userManager.ChangePasswordAsync(userLogin, model.PasswordAtual, model.NovaPassword);
                    if (!resultPass.Succeeded)
                    {
                        foreach (var erro in resultPass.Errors) ModelState.AddModelError("", erro.Description);
                        return RepopularView(model, participante);
                    }
                    precisaAtualizarLogin = true;
                }
                _context.Update(participante);
                await _context.SaveChangesAsync();

                if (userLogin != null) await _userManager.UpdateAsync(userLogin);

                // Se mudou email ou password, renova o cookie de login para o utilizador não ser desconectado
                if (precisaAtualizarLogin && userLogin != null)
                {
                    await _signInManager.RefreshSignInAsync(userLogin);
                }

                return RedirectToAction("Index", "Home");
            }

            return RepopularView(model, participante);
        }

        private IActionResult RepopularView(PerfilViewModel model, Participante p)
        {
            model.NomeAtual = p.Nome;
            model.EmailAtual = p.Email;
            return View(model);
        }

        // GET: Participantes/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var participante = await _context.Participantes.FirstOrDefaultAsync(m => m.Id == id);
            if (participante == null) return NotFound();

            // SEGURANÇA: Só o Organizador ou o próprio dono da conta podem ver esta página
            if (!User.IsInRole("Organizador") && participante.Email != User.Identity.Name)
            {
                return Forbid();
            }

            ViewBag.SelectedId = id;
            return View(participante);
        }

        // POST: Participantes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Procurar participante
            var participante = await _context.Participantes.FirstOrDefaultAsync(p => p.Id == id);
            if (participante == null) return NotFound();

            //Prevenir ataques via URL
            if (!User.IsInRole("Organizador") && participante.Email != User.Identity.Name)
            {
                return Forbid();
            }

            // Procurar utilizador no Identity
            var user = await _userManager.FindByEmailAsync(participante.Email);

            // Apagar participante
            _context.Participantes.Remove(participante);
            await _context.SaveChangesAsync(); // É mais seguro gravar o Participante primeiro

            // Apagar utilizador Identity
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            // Se foi o próprio utilizador a apagar a sua conta: Faz logout e vai para a Home
            if (participante.Email == User.Identity?.Name)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Index", "Home");
            }

            // Se foi o Organizador a apagar a conta de outra pessoa: Volta à lista
            return RedirectToAction(nameof(Index));
        }
    }
}
