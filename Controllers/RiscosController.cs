using Microsoft.AspNetCore.Mvc;
using VigiLant.Models;
using VigiLant.Contratos;
using Microsoft.AspNetCore.Authorization;
using VigiLant.Models.Enum;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VigiLant.Controllers
{
    [Authorize]
    public class RiscosController : Controller
    {
        private readonly IRiscoRepository _riscoRepository;
        private readonly IEquipamentoRepository _equipamentoRepository; 

        public RiscosController(IRiscoRepository riscoRepository, IEquipamentoRepository equipamentoRepository)
        {
            _riscoRepository = riscoRepository;
            _equipamentoRepository = equipamentoRepository;
        }

        // Helper para verificar se a requisição é AJAX (CRUCIAL para a modal)
        private bool IsAjaxRequest()
        {
            // Verifica o cabeçalho 'X-Requested-With' enviado pelo fetch/XMLHttpRequest
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        // GET: /Riscos/Index
        public IActionResult Index()
        {
            var riscos = _riscoRepository.GetAll();
            return View(riscos);
        }

        // GET: /Riscos/Create
        public IActionResult Create()
        {
            var equipamentos = _equipamentoRepository.GetAll();

            ViewData["EquipamentoId"] = new SelectList(
            equipamentos ?? new List<Equipamento>(), 
            "IdentificadorBroker",
            "Nome"
            );

            var novoRisco = new Risco
            {
                DataIdentificacao = DateTime.Today
            };

            if (IsAjaxRequest())
            {
                // SE AJAX: Retorna APENAS o conteúdo para a modal
                return PartialView("_CreateRiscoPartial", novoRisco);
            }
            // SE NÃO AJAX: Retorna a View completa
            return View(novoRisco);
        }

        // POST: /Riscos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Risco risco)
        {
            if (risco.DataIdentificacao == default(DateTime))
            {
                // Garante que a data seja definida se a validação falhar por causa do valor default.
                risco.DataIdentificacao = DateTime.Today;
            }

            if (ModelState.IsValid)
            {
                _riscoRepository.Add(risco);

                if (IsAjaxRequest())
                {
                    return Ok(); // Retorna Status 200/OK para o JavaScript (Sucesso)
                }
                return RedirectToAction(nameof(Index));
            }

            // SE VALIDAÇÃO FALHAR (ModelState.IsValid == false)
            if (IsAjaxRequest())
            {
                // Retorna Status 400 (Bad Request) para o JavaScript
                // O corpo da resposta é a Partial View com as mensagens de erro preenchidas
                Response.StatusCode = 400;
                return PartialView("_CreateRiscoPartial", risco);
            }
            return View(risco);
        }

        // GET: /Riscos/Details/5
        public IActionResult Details(int id)
        {
            var risco = _riscoRepository.GetById(id);
            if (risco == null)
            {
                if (IsAjaxRequest()) { Response.StatusCode = 404; return Content("Risco não encontrado."); }
                return NotFound();
            }

            if (IsAjaxRequest())
            {
                return PartialView("_DetailsRiscoPartial", risco);
            }
            return View(risco);
        }

        // GET: /Riscos/Edit/5
        public IActionResult Edit(int id)
        {
            var risco = _riscoRepository.GetById(id);
            if (risco == null)
            {
                if (IsAjaxRequest()) { Response.StatusCode = 404; return Content("Risco não encontrado."); }
                return NotFound();
            }

            if (IsAjaxRequest())
            {
                return PartialView("_EditRiscoPartial", risco);
            }
            return View(risco);
        }

        // POST: /Riscos/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Risco risco)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _riscoRepository.Update(risco);

                    if (IsAjaxRequest())
                    {
                        return Ok(); // Retorna Status 200/OK para o JavaScript (Sucesso)
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    // Lógica para tratar erros de banco de dados
                    ModelState.AddModelError(string.Empty, "Ocorreu um erro ao salvar as alterações.");

                    if (IsAjaxRequest())
                    {
                        Response.StatusCode = 400;
                        return PartialView("_EditRiscoPartial", risco);
                    }
                    return View(risco);
                }
            }

            // SE VALIDAÇÃO FALHAR
            if (IsAjaxRequest())
            {
                Response.StatusCode = 400;
                return PartialView("_EditRiscoPartial", risco);
            }
            return View(risco);
        }

        // GET: /Riscos/DeleteConfirmation/5 
        public IActionResult DeleteConfirmation(int id)
        {
            var risco = _riscoRepository.GetById(id);
            if (risco == null)
            {
                if (IsAjaxRequest())
                {
                    Response.StatusCode = 404;
                    return Content("Risco não encontrado para exclusão.");
                }
                return NotFound();
            }

            if (IsAjaxRequest())
            {
                return PartialView("_DeleteRiscoPartial", risco);
            }
            return View(risco);
        }

        // POST: /Riscos/Delete/5 (O ActionName="Delete" permite usar DeleteConfirmed no método)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _riscoRepository.Delete(id);

            if (IsAjaxRequest())
            {
                return Ok(); // Retorna Status 200/OK para o JavaScript (Sucesso)
            }
            return RedirectToAction(nameof(Index));
        }
    }
}