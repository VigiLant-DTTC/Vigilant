using Microsoft.AspNetCore.Mvc;
using VigiLant.Models;
using VigiLant.Contratos;
using Microsoft.AspNetCore.Authorization;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace VigiLant.Controllers
{
    [Authorize]
    public class RelatoriosController : Controller
    {
        private readonly IRelatorioRepository _relatorioRepository;
        private readonly IEquipamentoRepository _equipamentoRepository;
        private readonly IColaboradorRepository _colaboradorRepository;

        public RelatoriosController(IRelatorioRepository relatorioRepository, IEquipamentoRepository equipamentoRepository, IColaboradorRepository colaboradorRepository)
        {
            _relatorioRepository = relatorioRepository;
            _equipamentoRepository = equipamentoRepository;
            _colaboradorRepository = colaboradorRepository;
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        // GET: /Relatorios/Index
        public IActionResult Index()
        {
            var relatorios = _relatorioRepository.GetAll();
            return View(relatorios);
        }

        private void CarregarListaEquipamentos(string equipamentoNomeSelecionado = null)
        {
            // Puxa todos os equipamentos do repositório
            var equipamentos = _equipamentoRepository.GetAll();

            // Pega APENAS a propriedade Nome da entidade Equipamento
            var nomesEquipamentos = equipamentos
                                    .Select(e => e.Nome)
                                    .ToList();

            // Cria o SelectList usando a lista de strings
            ViewBag.EquipamentosLista = new SelectList(
                nomesEquipamentos, // Lista de strings (Nomes dos equipamentos)
                equipamentoNomeSelecionado // Item selecionado (útil para reter valor em caso de erro)
            );
        }

        // GET: /Relatorios/Create
        public IActionResult Create()
        {
            CarregarListaEquipamentos(); // <--- CHAMA A FUNÇÃO AGORA

            var novoRelatorio = new Relatorio
            {
                DataGeracao = DateTime.Today
            };

            if (IsAjaxRequest())
            {
                return PartialView("_CreateRelatorioPartial", novoRelatorio);
            }
            return View(novoRelatorio);
        }

        // POST: /Relatorios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Relatorio relatorio)
        {
            // ... (Lógica para obter GeradoPorColaboradorId inalterada)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                var colaborador = _colaboradorRepository.GetById(userId);
                if (colaborador != null)
                {
                    relatorio.GeradoPorColaboradorId = colaborador.Id;
                }
            }

            if (relatorio.DataGeracao == default(DateTime))
            {
                relatorio.DataGeracao = DateTime.Today;
            }

            if (ModelState.IsValid)
            {
                _relatorioRepository.Add(relatorio);

                if (IsAjaxRequest())
                {
                    return Ok();
                }
                return RedirectToAction(nameof(Index));
            }

            // CORREÇÃO: Recarrega a lista de Equipamentos usando o nome selecionado em caso de erro
            CarregarListaEquipamentos(relatorio.EquipamentoNome);

            if (IsAjaxRequest())
            {
                Response.StatusCode = 400;
                return PartialView("_CreateRelatorioPartial", relatorio);
            }
            return View(relatorio);
        }

        // GET: /Relatorios/Details/5
        public IActionResult Details(int id)
        {
            var relatorio = _relatorioRepository.GetById(id);
            if (relatorio == null)
            {
                if (IsAjaxRequest()) { Response.StatusCode = 404; return Content("Relatório não encontrado."); }
                return NotFound();
            }

            if (IsAjaxRequest())
            {
                return PartialView("_DetailsRelatorioPartial", relatorio);
            }
            return View(relatorio);
        }

        // GET: /Relatorios/Edit/5
        public IActionResult Edit(int id)
        {
            var relatorio = _relatorioRepository.GetById(id);
            if (relatorio == null)
            {
                if (IsAjaxRequest()) { Response.StatusCode = 404; return Content("Relatório não encontrado."); }
                return NotFound();
            }

            if (IsAjaxRequest())
            {
                return PartialView("_EditRelatorioPartial", relatorio);
            }
            return View(relatorio);
        }

        // POST: /Relatorios/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Relatorio relatorio)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _relatorioRepository.Update(relatorio);

                    if (IsAjaxRequest())
                    {
                        return Ok();
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, "Ocorreu um erro ao salvar as alterações.");

                    if (IsAjaxRequest())
                    {
                        Response.StatusCode = 400;
                        return PartialView("_EditRelatorioPartial", relatorio);
                    }
                    return View(relatorio);
                }
            }

            if (IsAjaxRequest())
            {
                Response.StatusCode = 400;
                return PartialView("_EditRelatorioPartial", relatorio);
            }
            return View(relatorio);
        }

        // GET: /Relatorios/DeleteConfirmation/5 
        public IActionResult DeleteConfirmation(int id)
        {
            var relatorio = _relatorioRepository.GetById(id);
            if (relatorio == null)
            {
                if (IsAjaxRequest())
                {
                    Response.StatusCode = 404;
                    return Content("Relatório não encontrado para exclusão.");
                }
                return NotFound();
            }

            if (IsAjaxRequest())
            {
                return PartialView("_DeleteRelatorioPartial", relatorio);
            }
            return View(relatorio);
        }

        // POST: /Relatorios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _relatorioRepository.Delete(id);

            if (IsAjaxRequest())
            {
                return Ok();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}