using System.ComponentModel.DataAnnotations;

namespace VigiLant.Models
{
    public class Relatorio
    {
        public int Id { get; set; }
        
        public string Nome { get; set; }
        
        public DateTime DataGeracao { get; set; } 
        
        public string Conteudo { get; set; } 
        
        public string TipoRelatorio { get; set; } 

        public string EquipamentoNome { get; set; } 
        
        public int GeradoPorColaboradorId { get; set; } 
    }
}