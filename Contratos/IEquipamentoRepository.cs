// Contratos/IEquipamentoRepository.cs

using System.Collections.Generic;
using VigiLant.Models;
using VigiLant.Models.Enum;

namespace VigiLant.Contratos
{
    public interface IEquipamentoRepository
    {
        // Métodos CRUD Padrão
        IEnumerable<Equipamento> GetAll();
        Equipamento? GetById(int id);
        void Add(Equipamento equipamento);
        void Update(Equipamento equipamento);
        void Delete(int id);

        // Método de Conexão: Registra o equipamento para monitoramento
        Equipamento Conectar(string identificadorUnico);

        // Método de Atualização: Chamado PELO SERVIÇO MQTT para salvar o dado real
        void AtualizarDadosEmTempoReal(int id, StatusEquipament status, string localizacao, string nome, TipoSensores tipoSensor, string ultimaMedicao);
    }
}