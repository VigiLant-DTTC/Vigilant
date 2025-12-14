// Repository/AppConfigRepository.cs

using VigiLant.Contratos;
using VigiLant.Models;
using VigiLant.Data;
using System.Linq;
using VigiLant.Models.Enum;

namespace VigiLant.Repository
{
    public class AppConfigRepository : IAppConfigRepository
    {
        private readonly BancoCtx _context;

        public AppConfigRepository(BancoCtx context)
        {
            _context = context;
        }

        public AppConfig? GetConfig()
        {
            var config = _context.AppConfigs.FirstOrDefault(); 

            if (config == null) // Se não encontrou nada
            {
                // 1. Cria uma configuração padrão
                config = new AppConfig
                {
                    Id = 1,
                    MqttHost = "broker.emqx.io", // Exemplo: Host local
                    MqttPort = MqttPorta.Porta1883, // Exemplo: 1883
                    MqttTopicWildcard = "vigilant/data/#"
                };

                // 2. Salva essa configuração no banco de dados
                _context.AppConfigs.Add(config);
                _context.SaveChanges();
            }

            return config;
        }

        public void UpdateConfig(AppConfig config)
        {
            var existing = GetConfig();
            if (existing != null)
            {
                existing.MqttHost = config.MqttHost;
                existing.MqttPort = config.MqttPort;
                existing.MqttTopicWildcard = config.MqttTopicWildcard;

                _context.AppConfigs.Update(existing);
            }
            else
            {
                config.Id = 1;
                _context.AppConfigs.Add(config);
            }
            _context.SaveChanges();
        }
    }
}