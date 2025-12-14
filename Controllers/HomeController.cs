using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VigiLant.Contratos;
using VigiLant.Models;
using VigiLant.Models.DTO;

namespace VigiLant.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IEquipamentoRepository _equipamentoR;
    private readonly IColaboradorRepository _colaboradorR;
    private readonly IRelatorioRepository _relatorioR;
    private readonly IRiscoRepository _riscoR;
    private readonly IAnaliseRepository _analiseR;


    public HomeController(ILogger<HomeController> logger, IEquipamentoRepository equipamentoR, IColaboradorRepository colaboradorR,
    IRelatorioRepository relatorioR, IRiscoRepository riscoR, IAnaliseRepository analiseR)
    {
        _logger = logger;
        _equipamentoR = equipamentoR;
        _colaboradorR = colaboradorR;
        _relatorioR = relatorioR;
        _riscoR = riscoR;
        _analiseR = analiseR;
    }

    private void CarregarEstatisticasDashboard()
    {
        var anoAtual = DateTime.Today.Year;
        var riscosMensais = _riscoR.GetRiscosPorMesNoAno(anoAtual).ToList();

        var dadosGrafico = Enumerable.Range(1, 12)
            .Select(m => new RiscoMensalDTO { Mes = m, Contagem = 0 })
            .ToList();

        // Substitui os valores existentes
        foreach (var dado in riscosMensais)
        {
            var index = dadosGrafico.FindIndex(d => d.Mes == dado.Mes);
            if (index != -1)
            {
                dadosGrafico[index].Contagem = dado.Contagem;
            }
        }

        // Passa a lista de contagens para a View (só os números)
        ViewBag.RiscosMensais = dadosGrafico.Select(d => d.Contagem).ToList();

        ViewBag.MaxAnalisesGeradas = 10;
    }


    public IActionResult Index()
    {
        CarregarEstatisticasDashboard();

        var todasAnalises = _analiseR.GetAll().ToList();
        var totalAnalises = todasAnalises.Count();

        var totalRiscos = _riscoR.GetAll().Count();

        var totalEquipamentos = _equipamentoR.GetAll().Count();

        var totalRelatorios = _relatorioR.GetAll().Count();

        var totalColaboradores = _colaboradorR.GetAll().Count();

        ViewBag.TotalRiscos = totalRiscos.ToString();
        ViewBag.TotalEquipamentos = totalEquipamentos.ToString();
        ViewBag.TotalRelatorios = totalRelatorios.ToString();
        ViewBag.TotalColaboradores = totalColaboradores.ToString();

        ViewBag.AnalisesUrgentes = todasAnalises
                                   .OrderByDescending(a => a.DataAnalise) // Exemplo: As mais recentes
                                   .Take(4) // Limita a 4 itens (como solicitado)
                                   .ToList();

        ViewBag.MaxAnalisesGeradas = totalAnalises.ToString();

        return View();
    }



}


