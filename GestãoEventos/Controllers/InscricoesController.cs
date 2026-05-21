using GestãoEventos.Data;
using GestãoEventos.Data.Classes;
using GestãoEventos.ViewModel.Eventos;
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

        // GET: Inscricoes
        public async Task<IActionResult> Index()
        {
            // 1. Guardar o email numa variável ANTES da consulta à base de dados
            var userEmail = User.Identity?.Name;

            // Prepara a consulta base
            var query = _context.Inscricoes
                .Include(i => i.Evento)
                .Include(i => i.Participante)
                .AsQueryable();

            // NOVA REGRA: Se NÃO for Organizador, filtra as inscrições
            if (!User.IsInRole("Organizador"))
            {
                // 2. Usar a variável no filtro e garantir que o Participante existe mesmo
                query = query.Where(i => i.Participante != null && i.Participante.Email == userEmail);
            }

            var inscricoes = await query.ToListAsync();

            return View(inscricoes);
        }

        private async Task<Inscricao?> DadosEventosParticipante(int? eventoId, int? participanteId)
        {
            return await _context.Inscricoes
                .Include(i => i.Evento)
                .Include(i => i.Participante)
                .FirstOrDefaultAsync(m => m.EventoId == eventoId && m.ParticipanteId == participanteId);
        }
        // GET: Inscricoes/Details/5
        public async Task<IActionResult> Details(int? eventoId, int? participanteId) // chave composta, por isso precisamos dos dois IDs para identificar a inscrição específica.
        {
            if (eventoId == null || participanteId == null) return NotFound();

            var inscricao = await DadosEventosParticipante(eventoId, participanteId);

            if (inscricao == null) return NotFound();

            if (!User.IsInRole("Organizador") && inscricao.Participante.Email != User.Identity.Name)
            {
                return Forbid(); // Dá erro 403 - Acesso Negado
            }

            return View(inscricao);
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
            model.EventosDisponiveis = new SelectList(_context.Eventos, "Id", "Nome", model.EventoId); //preenche a lista de eventos disponíveis para o dropdown
            ViewBag.SelectedId = id;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ObterDetalhesEvento(int id)
        {
            // 1. Vai buscar a entidade original à BD
            var eventoDb = await _context.Eventos.FindAsync(id);
            if (eventoDb == null) return NotFound();
            // 2. Mapeia manualmente para o ViewModel que a tua Partial View exige
            var viewModel = new EventoViewModel
            {
                Nome = eventoDb.Nome,
                Data = eventoDb.Data,
                Hora = eventoDb.Hora,
                Local = eventoDb.Local,
                Preco = eventoDb.Preco,
                Descricao = eventoDb.Descricao,
                Image = eventoDb.Image // Caminho da imagem string
            };

            // 3. Passa o ViewModel correto
            return PartialView("_DetailsEventoPartial", viewModel);
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

                return RedirectToAction("Index", "Eventos");
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
                ParticipanteId = inscricao.ParticipanteId
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
            return RedirectToAction(nameof(Index)); // Redireciona para a lista de inscrições após a exclusão
        }
    }
}
