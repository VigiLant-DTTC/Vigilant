using System.Collections.Generic;
using VigiLant.Models;

namespace VigiLant.Models.ViewModel
{
    public class NotificacoesViewModel
    {
        public List<Risco> NovosRiscos { get; set; } = new List<Risco>();

        public List<Analise> AnalisesGeradas { get; set; } = new List<Analise>();

        public IEnumerable<Colaborador> ColaboradoresPendentes { get; set; } = Enumerable.Empty<Colaborador>();
    }
}