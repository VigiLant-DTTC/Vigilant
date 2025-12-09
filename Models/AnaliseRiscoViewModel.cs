using System;
using System.Collections.Generic;

namespace VigiLant.Models
{
    // Model para armazenar o resultado de uma análise de IA
    public class AnaliseRiscoHistorico
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime DataAnalise { get; set; } = DateTime.Now;
        public int RiscosAnalisadosCount { get; set; }
        public int EquipamentosAnalisadosCount { get; set; }
        public string Resumo { get; set; } = string.Empty;
        public List<string> SugestoesSolucao { get; set; } = new List<string>();
        public List<string> NovosRiscosSugeridos { get; set; } = new List<string>();
        
        public bool IsLatest { get; set; } = false; 
    }

    // ViewModel atualizado para transportar o histórico de análises para a Partial View
    public class AnaliseRiscoViewModel
    {
        public List<AnaliseRiscoHistorico> HistoricoAnalises { get; set; } = new List<AnaliseRiscoHistorico>();
    }
}