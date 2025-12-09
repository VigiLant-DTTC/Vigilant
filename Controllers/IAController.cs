using Microsoft.AspNetCore.Mvc;
using VigiLant.Contratos;
using Microsoft.AspNetCore.Authorization;
using VigiLant.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using VigiLant.Models.Enum; // Garante acesso aos Enums, ex: NivelSeveridade

namespace VigiLant.Controllers
{
    [Authorize]
    public class IAController : Controller
    {
        private readonly IRiscoRepository _riscoRepository;
        
        // ** SIMULAÇÃO DE REPOSITÓRIO E PERSISTÊNCIA (IN-MEMORY STATIC) **
        // Em um projeto real, isso seria uma injeção de AnaliseRiscoRepository com acesso ao banco.
        private static List<AnaliseRiscoHistorico> AnaliseHistorico = new List<AnaliseRiscoHistorico>();
        // ** FIM DA SIMULAÇÃO **

        public IAController(IRiscoRepository riscoRepository)
        {
            _riscoRepository = riscoRepository;
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        // GET: /IA/AnaliseRiscos (Serve a tela principal de Análise e Histórico)
        public IActionResult AnaliseRiscos()
        {
            if (IsAjaxRequest())
            {
                var viewModel = new AnaliseRiscoViewModel
                {
                    // Ordena do mais recente para o mais antigo e limpa o destaque
                    HistoricoAnalises = AnaliseHistorico.OrderByDescending(a => a.DataAnalise).ToList() 
                };
                viewModel.HistoricoAnalises.ForEach(a => a.IsLatest = false); 
                
                return PartialView("_AnaliseRiscosPartial", viewModel);
            }
            return View(); 
        }

        // POST: /IA/ProcessarAnalise (Executa, salva e retorna o histórico atualizado)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProcessarAnalise()
        {
            var riscos = _riscoRepository.GetAll().ToList();
            var novoResultado = SimularAnaliseIA(riscos);

            // 1. Salva o novo resultado no histórico (Simulado)
            AnaliseHistorico.ForEach(a => a.IsLatest = false);
            novoResultado.DataAnalise = DateTime.Now; 
            novoResultado.IsLatest = true; 
            AnaliseHistorico.Add(novoResultado);

            // 2. Prepara o ViewModel atualizado
            var viewModel = new AnaliseRiscoViewModel
            {
                HistoricoAnalises = AnaliseHistorico.OrderByDescending(a => a.DataAnalise).ToList()
            };

            // 3. Retorna a Partial View atualizada (Status 200 para manter a modal aberta)
            if (IsAjaxRequest())
            {
                return PartialView("_AnaliseRiscosPartial", viewModel);
            }
            return RedirectToAction(nameof(AnaliseRiscos));
        }

        /**
         * Método Simulado para Processamento da Análise de IA (Gera Múltiplas Sugestões)
         */
        private AnaliseRiscoHistorico SimularAnaliseIA(List<Risco> riscos)
        {
            var resultado = new AnaliseRiscoHistorico
            {
                RiscosAnalisadosCount = riscos?.Count ?? 0,
                EquipamentosAnalisadosCount = riscos?.GroupBy(r => r.Equipamento).Select(g => g.Key).Count() ?? 0
            };

            if (riscos == null || !riscos.Any())
            {
                resultado.Resumo = "Nenhum risco foi encontrado no sistema para análise.";
                resultado.SugestoesSolucao.Add("Registre novos riscos no sistema VigiLant para iniciar a análise detalhada de IA.");
                resultado.NovosRiscosSugeridos.Add("N/A");
                return resultado;
            }

            var riscosAtivos = riscos.Where(r => r.Status == "Pendente" || r.Status == "Ativo").ToList();
            var equipamentosEmRisco = riscos.Select(r => r.Equipamento).Distinct().ToList();

            resultado.Resumo = $"Análise Completa: A IA analisou **{resultado.RiscosAnalisadosCount} riscos** em **{resultado.EquipamentosAnalisadosCount} equipamentos**. Foram identificados **{riscosAtivos.Count} riscos** que exigem ação imediata.";
            
            // --- GERAÇÃO DE SUGESTÕES DE SOLUÇÃO (Lendo "tudo sobre o risco") ---
            
            // 1. Ações Críticas Imediatas (Baseado em Gravidade)
            foreach (var risco in riscosAtivos.Where(r => r.NivelGravidade >= NivelSeveridade.Alto).OrderByDescending(r => r.NivelGravidade).Take(3))
            {
                resultado.SugestoesSolucao.Add($"**Ação Crítica (Risco {risco.Nome}):** Isolar ou desligar o equipamento '{risco.Equipamento}'. Implementar solução temporária para o risco de **{risco.TipoRisco}** em no máximo 24 horas.");
            }

            // 2. Revisão de Processos (Baseado em Tipo de Risco)
            foreach (var tipo in riscosAtivos.Select(r => r.TipoRisco).Distinct())
            {
                resultado.SugestoesSolucao.Add($"Revisar e validar o checklist de manutenção preventiva para todos os equipamentos envolvidos com o risco de **{tipo}**. Focar na causa raiz da '{tipo}'.");
            }
            
            // 3. Monitoramento e Controle
            if (riscosAtivos.Any(r => r.NivelGravidade == NivelSeveridade.Medio))
            {
                resultado.SugestoesSolucao.Add("Iniciar monitoramento contínuo de variáveis ambientais (temperatura, umidade, vibração) nos locais com riscos de nível Médio para antecipar a progressão.");
            }
            
            // 4. Treinamento
            resultado.SugestoesSolucao.Add("Treinar a equipe de operações nos procedimentos de emergência, com foco em simulações para os 5 riscos mais graves identificados na base de dados.");


            // --- GERAÇÃO DE FUTUROS RISCOS POTENCIAIS ---
            
            foreach (var equipamento in equipamentosEmRisco)
            {
                var tiposExistentes = riscos.Where(r => r.Equipamento == equipamento).Select(r => r.TipoRisco).ToList();

                if (!tiposExistentes.Contains(TipoRisco.Biologico))
                {
                    resultado.NovosRiscosSugeridos.Add($"**{equipamento}:** Risco de falha de infraestrutura (vazamento ou inundação) na área, visto a criticidade operacional do equipamento. **Recomendação:** Sensor de líquidos.");
                }

                if (tiposExistentes.Contains(TipoRisco.Fisico))
                {
                    resultado.NovosRiscosSugeridos.Add($"**{equipamento}:** Risco de Erro Humano devido à alta frequência de incidentes operacionais registrados. **Recomendação:** Implementar interface de usuário mais simples.");
                }
            }
            
            resultado.NovosRiscosSugeridos.Add("Risco de Obsoleto de Software/Firmware em 30% da frota de sensores. **Recomendação:** Planejar migração para novo sistema operacional em 6 meses.");
            resultado.NovosRiscosSugeridos.Add("Risco de Ataque Cibernético de Nível 3, devido a falhas de comunicação não criptografada entre equipamentos. **Recomendação:** Auditoria de segurança de rede.");


            if (!resultado.SugestoesSolucao.Any())
            {
                resultado.SugestoesSolucao.Add("O sistema está com baixo nível de risco. Manter a vigilância e realizar auditoria trimestral.");
            }

            return resultado;
        }
    }
}