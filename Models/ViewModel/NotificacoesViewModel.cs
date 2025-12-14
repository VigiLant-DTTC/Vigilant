using System.Collections.Generic;
using VigiLant.Models;

namespace VigiLant.Models.ViewModel
{
    public class NotificacoesViewModel
    {
        public List<Solicitacao> SolicitacoesPendentes { get; set; } = new List<Solicitacao>();

        public List<Risco> NovosRiscos { get; set; } = new List<Risco>();

        public List<Analise> AnalisesGeradas { get; set; } = new List<Analise>(); 
    }
}