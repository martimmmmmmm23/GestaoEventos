using GestãoEventos.Data;
using GestãoEventos.Data.Classes;
using GestãoEventos.ViewModel.Inscricoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GestãoEventos.Controllers
{
    public class InscricoesController : Controller
    {
        private readonly GestaoEventosDbContext _context;

        public InscricoesController(GestaoEventosDbContext context)
        {
            _context = context;
        }

        private async Task<Inscricao?> DadosEventosParticipante(int? eventoId, int? participanteId)
        {
            return await _context.Inscricoes
                .Include(i => i.Evento)
                .Include(i => i.Participante)
                .FirstOrDefaultAsync(m => m.EventoId == eventoId && m.ParticipanteId == participanteId);
        }

        // GET: Inscricoes/Create
        public async Task<IActionResult> Create(int? id)
        {
            var model = new InscricaoViewModel();

            // Se vier um ID de evento no link, tentamos pré-selecioná-lo
            if (id.HasValue)
            {
                var evento = await _context.Eventos.FindAsync(id);
                if (evento != null)
                {
                    model.EventoId = evento.Id;
                    model.NomeEvento = evento.Nome;
                    model.EventoSelecionado = evento;
                }
            }

            if (User.Identity != null && User.Identity.IsAuthenticated) // Injetar o email automaticamente
            {
                model.Email = User.Identity.Name;
            }

            CarregarDadosEventos(model);
            ViewBag.SelectedId = id;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CarregarDetalhes(InscricaoViewModel model)
        {
            var evento = await _context.Eventos.FindAsync(model.EventoId);
            if (evento != null)
            {
                model.EventoSelecionado = evento;
            }

            if (User.Identity != null && User.Identity.IsAuthenticated) // Injetar o email automaticamente
            {
                model.Email = User.Identity.Name;
            }

            // Limpa validações pendentes (como o Email em branco) porque ele só quer ver o evento
            ModelState.Clear();

            CarregarDadosEventos(model);

            return View("Create", model);
        }

        private void CarregarDadosEventos(InscricaoViewModel model)
        {
            model.EventosDisponiveis = new SelectList(
                _context.Eventos.Where(e => e.Data >= DateTime.Now),
                "Id",
                "Nome",
                model.EventoId
            );
        }

        // POST: Inscricoes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InscricaoViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (User.Identity != null && User.Identity.IsAuthenticated && !User.IsInRole("Organizador"))
                {
                    // Se o e-mail preenchido for diferente do e-mail de quem fez login
                    if (model.Email != User.Identity.Name)
                    {
                        ModelState.AddModelError("Email", "Não tem permissão para inscrever outros participantes.");
                        CarregarDadosEventos(model);
                        return View(model);
                    }
                }
                var evento = await _context.Eventos
                    .Include(e => e.Inscricoes)
                    .FirstOrDefaultAsync(e => e.Id == model.EventoId);

                if (evento == null || evento.Data < DateTime.Now)
                {
                    ModelState.AddModelError("EventoId", "O evento selecionado não existe ou já ocorreu.");
                    CarregarDadosEventos(model);
                    return View(model);
                }

                var participante = await _context.Participantes
                    .FirstOrDefaultAsync(p => p.Email == model.Email);

                if (participante == null)
                {
                    ModelState.AddModelError("Email", "Participante não encontrado. Verifique o e-mail ou crie conta.");

                    ViewBag.MostrarLinkRegisto = true;
                    CarregarDadosEventos(model);
                    return View(model);
                }


                if (evento.Inscricoes.Count >= evento.Lugares)
                {
                    ModelState.AddModelError("", "Não existem lugares disponíveis para este evento.");
                    CarregarDadosEventos(model);
                    return View(model);
                }

                // já inscrito
                bool jaInscrito = await _context.Inscricoes
                    .AnyAsync(i => i.EventoId == model.EventoId && i.ParticipanteId == participante.Id);

                if (jaInscrito)
                {
                    ModelState.AddModelError("Email", "Este e-mail já se encontra num registo neste evento.");
                    CarregarDadosEventos(model);
                    return View(model);
                }

                // criar inscrição
                var novaInscricao = new Inscricao
                {
                    EventoId = model.EventoId,
                    ParticipanteId = participante.Id
                };

                _context.Inscricoes.Add(novaInscricao);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
            }

            CarregarDadosEventos(model);
            return View(model);
        }

        private bool InscricaoExists(int eventoId, int participanteId)
        {
            return _context.Inscricoes.Any(e => e.EventoId == eventoId && e.ParticipanteId == participanteId);
        }

        // GET: Inscricoes/Delete/5
        [Authorize(Roles = "Organizador")]
        public async Task<IActionResult> Delete(int? eventoId, int? participanteId)
        {
            if (eventoId == null || participanteId == null) return NotFound();

            var inscricao = await DadosEventosParticipante(eventoId, participanteId);

            if (inscricao == null) return NotFound();

            var viewModel = new InscricaoViewModel
            {
                EventoId = inscricao.EventoId,
                ParticipanteId = inscricao.ParticipanteId,
                Nome = inscricao.Participante.Nome,
                Email = inscricao.Participante.Email,
                NomeEvento = inscricao.Evento.Nome
            };

            return View(viewModel);
        }

        // POST: Inscricoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Organizador")]
        public async Task<IActionResult> DeleteConfirmed(int? eventoId, int? participanteId)
        {
            var inscricao = await DadosEventosParticipante(eventoId, participanteId);

            if (inscricao != null)
            {
                _context.Inscricoes.Remove(inscricao);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Details", "Eventos", new { id = eventoId }); // Redireciona para a lista de inscrições após a exclusão
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Utilizador")]
        public async Task<IActionResult> DeleteParticipante(int? eventoId, int? participanteId)
        {
            var inscricao = await DadosEventosParticipante(eventoId, participanteId);

            if (inscricao != null)
            {
                _context.Inscricoes.Remove(inscricao);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
