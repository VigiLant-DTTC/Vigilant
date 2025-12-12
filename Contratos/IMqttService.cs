// VigiLant.Services/IMqttService.cs
using System.Threading.Tasks;

namespace VigiLant.Services
{
    public interface IMqttService
    {
        // Publica uma mensagem para o broker. Usado pelo EquipamentosController.
        Task PublishAsync(string topic, string payload, CancellationToken cancellationToken = default);
    }
}