using Microsoft.AspNetCore.Mvc;
using VigiLant.Models;
using VigiLant.Contratos;
using Microsoft.AspNetCore.Authorization;
using System;
using Microsoft.EntityFrameworkCore; // Para o Include

namespace VigiLant.Controllers
{
    [Authorize]
    public class AnalisesController : Controller
    {
        private readonly IAnaliseRepository _analiseRepository;
        private readonly IRiscoRepository _riscoRepository;
        private readonly IIAService _iaService;

        public AnalisesController(IAnaliseRepository analiseRepository, IRiscoRepository riscoRepository, IIAService iaService)
        {
            _analiseRepository = analiseRepository;
            _riscoRepository = riscoRepository;
            _iaService = iaService;
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        // GET: /Analises/Index
        public IActionResult Index()
        {
            var analises = _analiseRepository.GetAll();
            return View(analises);
        }

        public IActionResult SelectRisco()
        {
            var riscos = _riscoRepository.GetAll();

            if (IsAjaxRequest())
            {
                // Retorna a lista de riscos para a partial view de seleção
                return PartialView("_SelectRiscoPartial", riscos);
            }
            return View(riscos); // Retorno padrão, se necessário
        }

        // POST: /Analises/GerarAnalise/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GerarAnalise(int riscoId)
        {
            var risco = _riscoRepository.GetById(riscoId);
            if (risco == null)
            {
                // Se o risco não for encontrado, retorna erro 404
                if (IsAjaxRequest()) { return StatusCode(404, "Risco não encontrado para análise."); }
                return NotFound();
            }

            var resultadoIA = await _iaService.GerarAnaliseDeRisco(risco.Nome, risco.Descricao);

            var previsao = resultadoIA.Item1;
            var solucoes = resultadoIA.Item2;

            var novaAnalise = new Analise
            {
                RiscoId = risco.Id,
                DataAnalise = DateTime.Now,
                PrevisaoRiscosFuturos = previsao, 
                SolucoesSugeridas = solucoes,     
                StatusAnalise = "Gerada"
            };

            _analiseRepository.Add(novaAnalise);

            // Resposta de sucesso para o AJAX. O status 200/OK + um objeto JSON é ideal.
            if (IsAjaxRequest())
            {
                return Ok(new { success = true, analiseId = novaAnalise.Id, message = "Análise gerada com sucesso pela IA!" });
            }

            // Se não for AJAX, redireciona diretamente
            return RedirectToAction(nameof(Details), new { id = novaAnalise.Id });
        }

        // GET: /Analises/Details/5
        public IActionResult Details(int id)
        {
            var analise = _analiseRepository.GetById(id);
            if (analise == null)
            {
                if (IsAjaxRequest()) { Response.StatusCode = 404; return Content("Análise não encontrada."); }
                return NotFound();
            }

            if (IsAjaxRequest())
            {
                return PartialView("_DetailsAnalisePartial", analise);
            }
            return View(analise);
        }

        // GET: /Analises/DeleteConfirmation/5 
        public IActionResult DeleteConfirmation(int id)
        {
            var analise = _analiseRepository.GetById(id);
            if (analise == null)
            {
                if (IsAjaxRequest())
                {
                    Response.StatusCode = 404;
                    return Content("Análise não encontrada para exclusão.");
                }
                return NotFound();
            }

            if (IsAjaxRequest())
            {
                return PartialView("_DeleteAnalisePartial", analise);
            }
            return View(analise);
        }

        // POST: /Analises/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _analiseRepository.Delete(id);

            if (IsAjaxRequest())
            {
                return Ok(); // Retorna Status 200/OK para o JavaScript (Sucesso)
            }
            return RedirectToAction(nameof(Index));
        }
    }
}