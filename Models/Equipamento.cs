// Models/Equipamento.cs
using System;
using System.ComponentModel.DataAnnotations;
using VigiLant.Models.Enum;

namespace VigiLant.Models
{
    public class Equipamento
    {
        public int Id { get; set; }

        public string Nome { get; set; }

        public string Localizacao { get; set; }

        public StatusEquipament Status { get; set; }

        public DateTime UltimaAtualizacao { get; set; }

        public TipoSensores TipoSensor { get; set; }

        public string UltimaMedicao { get; set; }


        // Propriedade para o identificador único (Token/ID) usado na conexão com o broker
        public string IdentificadorBroker { get; set; }
    }
}