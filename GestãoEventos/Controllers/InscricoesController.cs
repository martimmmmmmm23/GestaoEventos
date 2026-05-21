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

        // GET: Inscricoes/Details/5
        public async Task<IActionResult> Details(int? eventoId, int? participanteId) // chave composta, por isso precisamos dos dois IDs para identificar a inscrição específica.
        {
            if (eventoId == null || participanteId == null) return NotFound();

            var inscricao = await _context.Inscricoes
                .Include(i => i.Evento)
                .Include(i => i.Participante)
                .FirstOrDefaultAsync(m => m.EventoId == eventoId && m.ParticipanteId == participanteId); //

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
                    ObterDetalhesEvento(id);
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

        // POST: Inscricoes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InscricaoViewModel model)
        {
            if (ModelState.IsValid)
            {
                var participante = await _context.Participantes
                    .FirstOrDefaultAsync(p => p.Email == model.Email);

                if (participante == null)
                {
                    // 1. Mantemos a mensagem de erro normal
                    ModelState.AddModelError("Email", "Participante não encontrado. Verifique o e-mail ou crie conta.");

                    // 2. NOVA LINHA: Enviamos um "sinal" para a View mostrar o link
                    ViewBag.MostrarLinkRegisto = true;

                    model.EventosDisponiveis = new SelectList(_context.Eventos, "Id", "Nome", model.EventoId);
                    return View(model);
                }

                bool jaInscrito = await _context.Inscricoes
                    .AnyAsync(i => i.EventoId == model.EventoId && i.ParticipanteId == participante.Id);

                if (jaInscrito)
                {
                    ModelState.AddModelError("Email", "Este e-mail já se encontra num registo neste evento.");
                    model.EventosDisponiveis = new SelectList(_context.Eventos, "Id", "Nome", model.EventoId);
                    return View(model);
                }

                var novaInscricao = new Inscricao
                {
                    EventoId = model.EventoId,
                    ParticipanteId = participante.Id
                };
                _context.Inscricoes.Add(novaInscricao);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Eventos");
            }

            model.EventosDisponiveis = new SelectList(_context.Eventos, "Id", "Nome", model.EventoId);
            return View(model);
        }

        //// GET: Inscricoes/Edit/5

        //[Authorize(Roles = "Organizador")] // Apenas organizadores podem editar
        //public async Task<IActionResult> Edit(int? eventoId, int? participanteId)
        //{
        //    if (eventoId == null || participanteId == null)
        //    {
        //        return NotFound();
        //    }

        //    var inscricao = await _context.Inscricoes
        //        .Include(i => i.Evento)
        //        .Include(i => i.Participante)
        //        .FirstOrDefaultAsync(m => m.EventoId == eventoId && m.ParticipanteId == participanteId);

        //    if (inscricao == null)
        //    {
        //        return NotFound();
        //    }

        //    // Preparamos as listas para as dropdowns, caso o organizador queira mudar o evento ou participante
        //    ViewData["EventoId"] = new SelectList(_context.Eventos, "Id", "Nome", inscricao.EventoId);
        //    ViewData["ParticipanteId"] = new SelectList(_context.Participantes, "Id", "Nome", inscricao.ParticipanteId);

        //    return View(inscricao);
        //}

        //// POST: Inscricoes/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Authorize(Roles = "Organizador")]
        //public async Task<IActionResult> Edit(int oldEventoId, int oldParticipanteId, [Bind("EventoId,ParticipanteId")] Inscricao inscricao)
        //{
        //    if (oldEventoId == 0 || oldParticipanteId == 0) return NotFound(); // Verifica se os IDs antigos existem na base de dados, caso contrário retorna NotFound

        //    if (inscricao.EventoId != oldEventoId || inscricao.ParticipanteId != oldParticipanteId) // Se o organizador mudou o evento ou participante, precisamos verificar se a nova combinação já existe para evitar insersoes duplicadas
        //    {
        //        if (InscricaoExists(inscricao.EventoId, inscricao.ParticipanteId)) // Verifica se já existe uma inscrição com a nova combinação de evento e participante. Se existir, adiciona um erro ao ModelState
        //        {
        //            ModelState.AddModelError("", "Este participante já está inscrito no evento selecionado.");

        //            ViewData["EventoId"] = new SelectList(_context.Eventos, "Id", "Nome", inscricao.EventoId);
        //            ViewData["ParticipanteId"] = new SelectList(_context.Participantes, "Id", "Nome", inscricao.ParticipanteId);
        //            return View(inscricao);
        //        }
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var inscricaoAntiga = await _context.Inscricoes // Procura a inscrição antiga usando os IDs antigos para garantir que estamos a editar a inscrição correta
        //                .FirstOrDefaultAsync(i => i.EventoId == oldEventoId && i.ParticipanteId == oldParticipanteId);

        //            if (inscricaoAntiga != null)
        //            {
        //                _context.Inscricoes.Remove(inscricaoAntiga);// Se a inscrição antiga for encontrada, ela é removida do banco de dados e substituida por uma nova inscrição com os novos IDs de evento e participante. Isso é necessário porque a chave primária da tabela de inscrições é composta pelos IDs de evento e participante, e não podemos simplesmente atualizar esses campos sem remover a inscrição antiga primeiro.
        //                await _context.SaveChangesAsync();
        //            }

        //            _context.Inscricoes.Add(inscricao);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException) //
        //        {
        //            if (!InscricaoExists(inscricao.EventoId, inscricao.ParticipanteId))
        //            {
        //                return NotFound();
        //            }
        //            else { throw; }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }

        //    ViewData["EventoId"] = new SelectList(_context.Eventos, "Id", "Nome", inscricao.EventoId);
        //    ViewData["ParticipanteId"] = new SelectList(_context.Participantes, "Id", "Nome", inscricao.ParticipanteId);
        //    return View(inscricao);
        //}

        private bool InscricaoExists(int eventoId, int participanteId)
        {
            return _context.Inscricoes.Any(e => e.EventoId == eventoId && e.ParticipanteId == participanteId);
        }

        // GET: Inscricoes/Delete/5
        [Authorize(Roles = "Organizador")]
        public async Task<IActionResult> Delete(int? eventoId, int? participanteId)
        {
            if (eventoId == null || participanteId == null) return NotFound();

            var inscricao = await _context.Inscricoes
                .Include(i => i.Evento)
                .Include(i => i.Participante)
                .FirstOrDefaultAsync(m => m.EventoId == eventoId && m.ParticipanteId == participanteId);

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
            var inscricao = await _context.Inscricoes
                .FirstOrDefaultAsync(m => m.EventoId == eventoId && m.ParticipanteId == participanteId);

            if (inscricao != null)
            {
                _context.Inscricoes.Remove(inscricao);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index)); // Redireciona para a lista de inscrições após a exclusão
        }
    }
}
