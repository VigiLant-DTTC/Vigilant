namespace VigiLant.Models.DTO
{
    public class RiscoMensalDTO
    {
        public int Mes { get; set; }        // Ex: 1 (Janeiro), 2 (Fevereiro), etc.
        public int Contagem { get; set; }   // Número de riscos identificados
    }
}