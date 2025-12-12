// Controllers/EquipamentosController.cs
// (CÓDIGO COMPLETO E AJUSTADO)

using Microsoft.AspNetCore.Mvc;
using VigiLant.Models;
using VigiLant.Contratos;
using Microsoft.AspNetCore.Authorization;
using VigiLant.Models.Enum;
using System;
using System.Threading.Tasks;
using VigiLant.Services;
using VigiLant.Repository;

namespace VigiLant.Controllers
{
    [Authorize]
    public class EquipamentosController : Controller
    {
        private readonly IEquipamentoRepository _equipamentoRepository;

        private readonly IMqttService _mqttService;
        private readonly IAppConfigRepository _appConfigRepository;
        public EquipamentosController(IEquipamentoRepository equipamentoRepository, IMqttService mqttService, IAppConfigRepository appConfigRepository)
        {
            _equipamentoRepository = equipamentoRepository;
            _mqttService = mqttService;
            _appConfigRepository = appConfigRepository;
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        // GET: /Equipamentos/Index
        public IActionResult Index()
        {
            var equipamentos = _equipamentoRepository.GetAll();
            return View(equipamentos);
        }

        // GET: /Equipamentos/Conectar
        public async Task<IActionResult> Conectar()
        {
            if (IsAjaxRequest())
            {
                return PartialView("_ConectarEquipamentoPartial", new Equipamento());
            }
            return View(new Equipamento());
        }

        // POST: /Equipamentos/Conectar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Conectar(string identificador)
        {
            if (string.IsNullOrEmpty(identificador))
            {
                ModelState.AddModelError("identificador", "O identificador é obrigatório.");
                return PartialView("_ConectarEquipamentoPartial", new Equipamento());
            }

            try
            {
                // 1. Cadastra o equipamento no DB com status inicial 'AguardandoDados'
                var novoEquipamento = _equipamentoRepository.Conectar(identificador);

                // 2. Carrega as configurações do broker (Topic Wildcard para publicação)
                var config = _appConfigRepository.GetConfig();
                if (config == null)
                {
                    throw new InvalidOperationException("Configuração do broker (AppConfig) não encontrada.");
                }

                // 3. Monta o tópico de comando (ex: vigilant/command/SENS_01)
                // Usamos a parte antes do '#' do MqttTopicWildcard
                var baseTopic = config.MqttTopicWildcard.Split('/')[0]; // Ex: "vigilant"
                var publishTopic = $"{baseTopic}/command/{identificador}"; // Ex: vigilant/command/SENS_01

                // 4. Manda o comando "Conectar" para o broker, que o dispositivo deve ouvir.
                await _mqttService.PublishAsync(publishTopic, "CONECTAR");

                if (IsAjaxRequest())
                {
                    // Retorna Status 200/OK para o JavaScript (Sucesso)
                    return Ok(new { equipamentoId = novoEquipamento.Id, nome = novoEquipamento.Nome });
                }
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                // Captura o erro do MQTT Service se não estiver conectado ou erro de DB
                ModelState.AddModelError(string.Empty, $"Falha: {ex.Message}");
                return PartialView("_ConectarEquipamentoPartial", new Equipamento() { IdentificadorBroker = identificador });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erro inesperado ao conectar: {ex.Message}");
                return PartialView("_ConectarEquipamentoPartial", new Equipamento() { IdentificadorBroker = identificador });
            }
        }

        // ---------------------------------------------
        // NOVO: VISUALIZAR DETALHES (Read)
        // ---------------------------------------------

        // GET: /Equipamentos/Details/5
        public IActionResult Details(int id)
        {
            var equipamento = _equipamentoRepository.GetById(id);
            if (equipamento == null)
            {
                if (IsAjaxRequest())
                {
                    Response.StatusCode = 404;
                    return Content("Equipamento não encontrado.");
                }
                return NotFound();
            }

            if (IsAjaxRequest())
            {
                // Retorna a view parcial de detalhes
                return PartialView("_DetailsEquipamentoPartial", equipamento);
            }
            return View(equipamento);
        }

        // GET: /Equipamentos/Monitorar/5
        public IActionResult Monitorar(int id)
        {
            var equipamento = _equipamentoRepository.GetById(id);
            if (equipamento == null)
            {
                if (IsAjaxRequest())
                {
                    Response.StatusCode = 404;
                    return Content("Equipamento não encontrado.");
                }
                return NotFound();
            }

            if (IsAjaxRequest())
            {
                return PartialView("_MonitorarEquipamentoPartial", equipamento);
            }
            return View(equipamento);
        }

        // GET: /Equipamentos/GetRealTimeData/5
        [HttpGet]
        public IActionResult GetRealTimeData(int id)
        {
            var equipamento = _equipamentoRepository.GetById(id);
            if (equipamento == null)
            {
                return NotFound();
            }

            // Retorna um JSON com os dados ATUALIZADOS DO BANCO
            return Json(new
            {
                nome = equipamento.Nome,
                localizacao = equipamento.Localizacao,
                status = equipamento.Status.ToString(),
                tipoSensor = equipamento.TipoSensor.ToString(),
                ultimaAtualizacao = equipamento.UltimaAtualizacao.ToString("dd/MM/yyyy HH:mm:ss"),
                valorMedicao = equipamento.UltimaMedicao
            });
        }

        // ---------------------------------------------
        // NOVO: EXCLUIR/DESCONECTAR (Delete)
        // ---------------------------------------------

        // GET: /Equipamentos/DeleteConfirmation/5 
        public IActionResult DeleteConfirmation(int id)
        {
            var equipamento = _equipamentoRepository.GetById(id);
            if (equipamento == null)
            {
                if (IsAjaxRequest())
                {
                    Response.StatusCode = 404;
                    return Content("Equipamento não encontrado para exclusão.");
                }
                return NotFound();
            }

            if (IsAjaxRequest())
            {
                // Retorna a view parcial de confirmação de exclusão
                return PartialView("_DeleteEquipamentoPartial", equipamento);
            }
            return View(equipamento);
        }

        // POST: /Equipamentos/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _equipamentoRepository.Delete(id);

            // Opcional: Se necessário, enviar uma mensagem de "desativação" ao Broker aqui.

            if (IsAjaxRequest())
            {
                // Retorna Status 200/OK para o JavaScript (Sucesso)
                return Ok();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}