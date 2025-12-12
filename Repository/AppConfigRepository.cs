// Repository/AppConfigRepository.cs

using VigiLant.Contratos;
using VigiLant.Models;
using VigiLant.Data;
using System.Linq;

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
            return _context.AppConfigs.FirstOrDefault();
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