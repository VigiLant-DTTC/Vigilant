using Microsoft.AspNetCore.Mvc;
using VigiLant.Models;
using VigiLant.Contratos;
using Microsoft.AspNetCore.Authorization;
using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace VigiLant.Controllers
{
    [Authorize]
    public class ColaboradoresController : Controller
    {
        private readonly IColaboradorRepository _colaboradorRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ISolicitacaoRepository _solicitacaoRepository;

        public ColaboradoresController(IColaboradorRepository colaboradorRepository, IUsuarioRepository usuarioRepository, ISolicitacaoRepository solicitacaoRepository)
        {
            _colaboradorRepository = colaboradorRepository;
            _usuarioRepository = usuarioRepository;
            _solicitacaoRepository = solicitacaoRepository;
        }

        // Helper para verificar se a requisição é AJAX
        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        // GET: /Colaboradores/Index
        public IActionResult Index()
        {
            var colaboradores = _colaboradorRepository.GetAll();
            return View(colaboradores);
        }

        // GET: /Colaboradores/Create -> Retorna a Partial
        public IActionResult Create(int? solicitacaoId)
        {
            var novoColaborador = new Colaborador { DataAdmissao = DateTime.Today };

            var nomePreenchimento = TempData["NomeSolicitacao"] as string;
            var emailPreenchimento = TempData["EmailSolicitacao"] as string;

            if (solicitacaoId.HasValue)
            {
                var solicitacao = _solicitacaoRepository.GetById(solicitacaoId.Value);

                if (solicitacao != null && solicitacao.Status == VigiLant.Models.Enum.StatusSolicitacao.Pendente)
                {
                    // 2. Preenche o modelo Colaborador
                    novoColaborador.Email = solicitacao.Email;
                    novoColaborador.Nome = solicitacao.Nome;

                    // 3. Passa o ID da Solicitação para a View para o POST
                    ViewData["SolicitacaoId"] = solicitacao.Id;
                }
            }

            if (IsAjaxRequest())
            {
                return PartialView("_CreateColaboradorPartial", novoColaborador);
            }
            return View(novoColaborador);
        }

        // POST: /Colaboradores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Colaborador colaborador)
        {
            if (ModelState.IsValid)
            {
                _colaboradorRepository.Add(colaborador);

                if (IsAjaxRequest())
                {
                    // Retorna Ok com o TempData para ser exibido após o reload
                    if (TempData["Sucesso"] != null)
                    {
                        return Ok(new { success = true, message = TempData["Sucesso"] });
                    }
                    return Ok();
                }
                return RedirectToAction(nameof(Index));
            }

            if (IsAjaxRequest())
            {
                Response.StatusCode = 400; // Erro de Validação

                var solicitacaoIdValue = Request.Form["SolicitacaoId"];
                if (!string.IsNullOrEmpty(solicitacaoIdValue))
                {
                    ViewData["SolicitacaoId"] = solicitacaoIdValue;
                }

                return PartialView("_CreateColaboradorPartial", colaborador);
            }
            return View(colaborador);
        }

        // GET: /Colaboradores/Details/5 -> Retorna a Partial
        public IActionResult Details(int id)
        {
            var colaborador = _colaboradorRepository.GetById(id);
            if (colaborador == null) { return NotFound(); }

            if (IsAjaxRequest())
            {
                return PartialView("_DetailsColaboradorPartial", colaborador);
            }
            return View(colaborador);
        }

        // GET: /Colaboradores/Edit/5 -> Retorna a Partial
        public IActionResult Edit(int id)
        {
            var colaborador = _colaboradorRepository.GetById(id);
            if (colaborador == null) { return NotFound(); }

            if (IsAjaxRequest())
            {
                return PartialView("_EditColaboradorPartial", colaborador);
            }
            return View(colaborador);
        }

        // POST: /Colaboradores/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Mude o retorno para Task<IActionResult> e adicione 'async'
        public async Task<IActionResult> Edit(Colaborador colaborador)
        {
            if (ModelState.IsValid)
            {
                // 1. Atualiza o Colaborador (Cargo, Nome, etc.)
                _colaboradorRepository.Update(colaborador); // [cite: 5]

                bool cargoAtualizadoParaUsuarioLogado = false;

                // 2. Propaga a mudança de Cargo para o Usuário vinculado
                if (colaborador.UsuarioId.HasValue)
                {
                    // Chama o método assíncrono para atualizar o cargo do usuário
                    await _usuarioRepository.UpdateCargo(colaborador.UsuarioId.Value, colaborador.Cargo); // [cite: 5]

                    // 3. Verifica se o usuário logado é o colaborador que teve o cargo alterado
                    var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

                    if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim.Value, out int userId) && userId == colaborador.UsuarioId.Value)
                    {
                        cargoAtualizadoParaUsuarioLogado = true;
                    }
                }

                if (IsAjaxRequest()) { return Ok(); }

                // 4. Se o usuário logado teve seu cargo alterado, força o logout e redireciona para o login com aviso.
                if (cargoAtualizadoParaUsuarioLogado)
                {
                    // Limpar o cookie de autenticação
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    TempData["Sucesso"] = "Seu cargo foi atualizado. Por favor, faça login novamente para que o novo cargo seja aplicado.";
                    return RedirectToAction("Login", "Conta");
                }

                return RedirectToAction(nameof(Index)); // [cite: 5]
            }

            if (IsAjaxRequest())
            {
                Response.StatusCode = 400;
                return PartialView("_EditColaboradorPartial", colaborador); // Retorna a Partial com erros
            }
            return View(colaborador);
        }

        // GET: /Colaboradores/DeleteConfirmation/5 -> Retorna a Partial
        public IActionResult DeleteConfirmation(int id)
        {
            var colaborador = _colaboradorRepository.GetById(id);
            if (colaborador == null) { return NotFound(); }

            if (IsAjaxRequest())
            {
                return PartialView("_DeleteColaboradorPartial", colaborador);
            }
            return View(colaborador);
        }

        // POST: /Colaboradores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var colaborador = _colaboradorRepository.GetById(id);

            if (colaborador != null)
            {
                // 1. Verificar se há um Usuario vinculado
                if (colaborador.UsuarioId.HasValue)
                {
                    await _usuarioRepository.Delete(colaborador.UsuarioId.Value);
                }

                // 3. Excluir o Colaborador
                _colaboradorRepository.Delete(id);
            }

            if (IsAjaxRequest()) { return Ok(); } // Sucesso AJAX
            return RedirectToAction(nameof(Index));
        }
    }
}