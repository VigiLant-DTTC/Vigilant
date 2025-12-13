using VigiLant.Models.Enum;

namespace VigiLant.Config
{
    // Estrutura para definir os limites de risco para um tipo de sensor
    public class LimiteSensor
    {
        public double LimiteAlerta { get; set; }
        public double LimitePerigo { get; set; }
        public double LimiteCritico { get; set; }
        public string Unidade { get; set; }
    }

    public static class LimitesMedicao
    {
        public static Dictionary<TipoSensores, LimiteSensor> Regras = new Dictionary<TipoSensores, LimiteSensor>
        {
            {
                TipoSensores.Temperatura,
                new LimiteSensor
                {
                    LimiteAlerta = 100.0,    // Acima de 100
                    LimitePerigo = 250.0,    // Acima de 250
                    LimiteCritico = 350.0,   // Acima de 350
                    Unidade = "°C"
                }
            },
            // Exemplo 2: Sensor de Corrente (Amperes)
            {
                TipoSensores.CorrenteEletrica,
                new LimiteSensor
                {
                    LimiteAlerta = 40.0,     // Acima de 40A
                    LimitePerigo = 50.0,     // Acima de 50A
                    LimiteCritico = 70.0,    // Acima de 70A
                    Unidade = "A"
                }
            },
            // Adicione mais regras conforme seus TiposSensores
        };
        
        // Novo método para obter o risco com base na medição
        public static (bool IsRisco, NivelSeveridade Nivel, string NomeRisco) AvaliarMedicao(TipoSensores tipoSensor, double valor)
        {
            if (!Regras.ContainsKey(tipoSensor))
            {
                return (false, NivelSeveridade.Baixo, string.Empty);
            }

            var regra = Regras[tipoSensor];

            if (valor >= regra.LimiteCritico)
            {
                return (true, NivelSeveridade.Critico, $"{tipoSensor} Excedeu {regra.LimiteCritico} {regra.Unidade}");
            }
            if (valor >= regra.LimitePerigo)
            {
                return (true, NivelSeveridade.Alto, $"{tipoSensor} Excedeu {regra.LimitePerigo} {regra.Unidade}");
            }
            if (valor >= regra.LimiteAlerta)
            {
                return (true, NivelSeveridade.Medio, $"{tipoSensor} Excedeu {regra.LimiteAlerta} {regra.Unidade}");
            }
            
            
            return (false, NivelSeveridade.Baixo, string.Empty);
        }
    }
}