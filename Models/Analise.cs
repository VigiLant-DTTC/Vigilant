using System.ComponentModel.DataAnnotations;

namespace VigiLant.Models
{
    public class Analise
    {
        public int Id { get; set; }
        public int RiscoId { get; set; }
        public Risco Risco { get; set; }
        
        [Display(Name = "Data da Análise")]
        [DataType(DataType.Date)]
        public DateTime DataAnalise { get; set; } = DateTime.Today;

        [Display(Name = "Previsão de Riscos Futuros (IA)")]
        public string PrevisaoRiscosFuturos { get; set; } = string.Empty;

        [Display(Name = "Soluções Sugeridas (IA)")]
        public string SolucoesSugeridas { get; set; } = string.Empty;

        [Display(Name = "Status da Análise")]
        public string StatusAnalise { get; set; } = "Gerada";
    }
}