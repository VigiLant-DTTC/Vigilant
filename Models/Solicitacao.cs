using System;
using VigiLant.Models.Enum;

namespace VigiLant.Models
{
    public class Solicitacao
    {
        public int Id { get; set; }
        
        public string Nome { get; set; } = string.Empty; 

        public string Email { get; set; } = string.Empty; 

        public DateTime DataSolicitacao { get; set; }

        public StatusSolicitacao Status { get; set; } 
    }
}