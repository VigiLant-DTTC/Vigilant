using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using VigiLant.Models.ViewModel;
using VigiLant.Contratos;
using VigiLant.Models;
using VigiLant.Models.Enum;
using System.Security.Claims;

namespace VigiLant.Controllers
{
    // Apenas usuários logados podem acessar as notificações
    [Authorize]
    public class NotificacoesController : Controller
    {
        private readonly IRiscoRepository _riscoRepository;
        private readonly IAnaliseRepository _analiseRepository;
        private readonly IColaboradorRepository _colaboradorRepository;

        // Injeção de dependência dos repositórios
        public NotificacoesController(
            IRiscoRepository riscoRepository,
            IAnaliseRepository analiseRepository,
            IColaboradorRepository colaboradorRepository)
        {
            _riscoRepository = riscoRepository;
            _analiseRepository = analiseRepository;
            _colaboradorRepository = colaboradorRepository;
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        // GET: /Notificacoes/Index
        public IActionResult Index()
        {
            var viewModel = new NotificacoesViewModel();

            // 1. Notificações de SOLICITAÇÕES (CONFIRMAÇÃO DE ACESSO e Fluxo Antigo)
            if (User.HasClaim(ClaimTypes.Role, Cargo.Administrador.ToString()))
            {
                // NOVO: Busca colaboradores que se registraram e aguardam confirmação
                viewModel.ColaboradoresPendentes = _colaboradorRepository.GetAll()
                    .Where(c => c.StatusAcesso == StatusVinculacao.aConfirmar)
                    .ToList();
            }
            else
            {
                // Inicializa as listas do Admin para não dar erro na View
                viewModel.ColaboradoresPendentes = new List<Colaborador>();
            }


            // 2. Notificações de Riscos (Status Pendente)
            viewModel.NovosRiscos = _riscoRepository.GetAll().Where(r => r.Status == StatusRisco.Pendente).ToList();

            // 3. Notificações de Análises (20 mais recentes)
            viewModel.AnalisesGeradas = _analiseRepository.GetAll()
                                        .OrderByDescending(a => a.DataAnalise)
                                        .Take(20).ToList();

            ViewData["Title"] = "Notificações";
            return View(viewModel);
        }
        
        // POST: /Notificacoes/MarcarRiscoComoVisualizado/5
       [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarcarRiscoComoVisualizado(int id)
        {
            var risco = _riscoRepository.GetById(id);

            if (risco == null) { return NotFound(); }

            // Lógica para marcar o risco como 'Em Analise'
            if (risco.Status == StatusRisco.Pendente)
            {
                risco.Status = StatusRisco.EmAndamento;
                _riscoRepository.Update(risco);
            }

            TempData["Sucesso"] = $"Risco {risco.Nome} marcado como 'Em Análise'.";
            return RedirectToAction(nameof(Index));
        }
    }
}