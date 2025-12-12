using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Client;
using Newtonsoft.Json;
using VigiLant.Contratos;
using VigiLant.Models.Payload;
using VigiLant.Models.Enum;
using Microsoft.AspNetCore.SignalR;
using VigiLant.Hubs;

namespace VigiLant.Services
{
    public class MqttClientService : BackgroundService, IMqttService
    {
        private readonly ILogger<MqttClientService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private IMqttClient _mqttClient;
        private MqttFactory _mqttFactory;
        private readonly IHubContext<MedicaoHub> _medicaoHubContext;

        // Propriedades para as configurações que serão carregadas do DB
        private string _mqttHost;
        private int _mqttPort;
        private string _mqttTopicWildcard;

        public MqttClientService(ILogger<MqttClientService> logger, IServiceProvider serviceProvider, IHubContext<MedicaoHub> medicaoHubContext)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _medicaoHubContext = medicaoHubContext;
            _mqttFactory = new MqttFactory();
            _mqttClient = _mqttFactory.CreateMqttClient();

            _mqttClient.ApplicationMessageReceivedAsync += HandleApplicationMessageReceivedAsync;
            _mqttClient.DisconnectedAsync += HandleDisconnectedAsync;
        }

        public async Task PublishAsync(string topic, string payload, CancellationToken cancellationToken = default)
        {
            if (!_mqttClient.IsConnected)
            {
                _logger.LogWarning($"Não foi possível publicar no tópico '{topic}': Cliente MQTT não está conectado.");
                // Lança exceção para o Controller capturar e notificar o usuário (IMPORTANTE!)
                throw new InvalidOperationException("Cliente MQTT não está conectado. Tente recarregar a página ou verifique as configurações do broker.");
            }

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(false)
                .Build();

            await _mqttClient.PublishAsync(message, cancellationToken);
            _logger.LogInformation($"Mensagem publicada no tópico: {topic} com payload: {payload}");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!_mqttClient.IsConnected)
                {
                    _logger.LogInformation("Cliente MQTT tentando se conectar...");
                    try
                    {
                        // 1. Carregar a configuração do banco de dados (AppConfig)
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var configRepo = scope.ServiceProvider.GetRequiredService<IAppConfigRepository>();
                            var config = configRepo.GetConfig();

                            if (config != null)
                            {
                                _mqttHost = config.MqttHost;
                                _mqttPort = (int)config.MqttPort;
                                _mqttTopicWildcard = config.MqttTopicWildcard;
                            }
                            else
                            {
                                _logger.LogError("Configuração do AppConfig não encontrada.");
                                await Task.Delay(5000, stoppingToken);
                                continue;
                            }
                        }

                        // 2. Tentar Conectar
                        var options = new MqttClientOptionsBuilder()
                            .WithTcpServer(_mqttHost, _mqttPort)
                            .WithClientId($"VigiLantServer_{Guid.NewGuid()}")
                            .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
                            .Build();

                        await _mqttClient.ConnectAsync(options, stoppingToken);

                        // 3. Subscrever
                        if (_mqttClient.IsConnected)
                        {
                            // Usa o SubscribeOptionsBuilder (corrigindo o erro de compilação anterior)
                            var subscribeOptions = _mqttFactory.CreateSubscribeOptionsBuilder()
                                .WithTopicFilter(f => f
                                    .WithTopic(_mqttTopicWildcard) // vigilant/data/#
                                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce))
                                .Build();

                            await _mqttClient.SubscribeAsync(subscribeOptions, stoppingToken);

                            _logger.LogInformation($"Cliente MQTT conectado e subscrito em: {_mqttTopicWildcard}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Falha na conexão/subscrição MQTT. Tentando novamente em 5s.");
                    }
                }
                await Task.Delay(5000, stoppingToken);
            }
        }

        private async Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs arg)
        {
            _logger.LogWarning($"Desconectado do Broker MQTT. Tentando reconectar em 5 segundos...");
            // Não precisamos de um await aqui, o loop ExecuteAsync cuidará da reconexão.
            await Task.Delay(TimeSpan.FromSeconds(5));
        }

        private async Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment.ToArray());
            _logger.LogInformation($"Mensagem MQTT recebida no tópico {e.ApplicationMessage.Topic}: {payload}");

            try
            {
                // Desserializa o payload para o modelo RealTimeDataPayload
                var data = JsonConvert.DeserializeObject<RealTimeDataPayload>(payload);

                if (data != null)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var repo = scope.ServiceProvider.GetRequiredService<IEquipamentoRepository>();

                        // Busca o equipamento usando o IdentificadorBroker do payload
                        var equipamento = repo.GetAll().FirstOrDefault(eq => eq.IdentificadorBroker == data.Identificador);

                        if (equipamento != null)
                        {
                            // Chamada ao Repositório para atualizar os dados
                            repo.AtualizarDadosEmTempoReal(
                                equipamento.Id,
                                (StatusEquipament)data.Status,
                                data.Localizacao,
                                data.Nome,
                                (TipoSensores)data.TipoSensor,
                                data.ValorMedicao // <--- NOVO: Inclui o valor da medição
                            );
                            _logger.LogInformation($"Equipamento #{equipamento.Id} ({data.Identificador}) atualizado com dados reais.");

                            // Publica no SignalR para atualizar a interface (Index.cshtml)
                            var novaMedicao = new
                            {
                                Id = equipamento.Id,
                                Nome = data.Nome,
                                Localizacao = data.Localizacao,
                                Status = (StatusEquipament)data.Status,
                                UltimaAtualizacao = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                                ValorMedicao = data.ValorMedicao
                            };

                            await _medicaoHubContext.Clients.All.SendAsync("ReceberAtualizacaoEquipamento", novaMedicao);
                        }
                        else
                        {
                            _logger.LogWarning($"Dados recebidos para identificador não cadastrado: {data.Identificador}");
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"Erro ao desserializar payload MQTT: {payload}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro geral ao processar mensagem MQTT.");
            }
            await Task.CompletedTask;
        }
    }
}