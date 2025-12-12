using VigiLant.Models.Enum;

namespace VigiLant.Models
{
    public class Colaborador
    {
         public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public Cargo Cargo { get; set; }
        public string Departamento { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public DateTime DataAdmissao { get; set; }

        // --- VINCULAÇÃO ---
        public int? UsuarioId { get; set; }
        public StatusVinculacao StatusAcesso { get; set; }
    }
}