using System.ComponentModel.DataAnnotations;
using VigiLant.Models.Enum;

namespace VigiLant.Models
{
    public class Risco
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Equipamento {get;set;}
        public TipoRisco TipoRisco {get; set;}
        public NivelSeveridade NivelGravidade { get; set; } 
        public StatusRisco Status { get; set; } = StatusRisco.Pendente;
        public DateTime DataIdentificacao { get; set; }
    }
}