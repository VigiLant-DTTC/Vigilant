namespace VigiLant.Models.Payload
{
    public class RealTimeDataPayload
    {
        public string Identificador { get; set; }
        public string Nome { get; set; }
        public string Localizacao { get; set; }
        public int TipoSensor { get; set; }
        public int Status { get; set; }
        public string ValorMedicao { get; set; }
    }
}