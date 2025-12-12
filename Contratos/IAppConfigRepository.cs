// Contratos/IAppConfigRepository.cs

using VigiLant.Models;

namespace VigiLant.Contratos
{
    public interface IAppConfigRepository
    {
        AppConfig? GetConfig();
        void UpdateConfig(AppConfig config);
    }
}