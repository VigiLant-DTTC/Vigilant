// Repository/EquipamentoRepository.cs

using VigiLant.Contratos;
using VigiLant.Models;
using VigiLant.Models.Enum;
using VigiLant.Data; // Importar BancoCtx
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System;

namespace VigiLant.Repository
{
    // A classe implementa a interface
    public class EquipamentoRepository : IEquipamentoRepository
    {
        private readonly BancoCtx _context;

        public EquipamentoRepository(BancoCtx context)
        {
            _context = context;
        }

        // Lógica para encontrar o próximo ID disponível (mantendo seu padrão)
        private int GetNextAvailableId()
        {
            var existingIds = _context.Equipamentos
                                     .Select(e => e.Id)
                                     .OrderBy(id => id)
                                     .ToList();

            if (!existingIds.Any())
            {
                return 1;
            }

            int nextId = 1;

            foreach (var id in existingIds)
            {
                if (id > nextId)
                {
                    return nextId;
                }
                nextId = id + 1;
            }

            return nextId;
        }

        // --- MÉTODOS CRUD ---

        public IEnumerable<Equipamento> GetAll()
        {
            return _context.Equipamentos.ToList();
        }

        public Equipamento? GetById(int id)
        {
            return _context.Equipamentos.Find(id);
        }

        public Equipamento Conectar(string identificadorUnico)
        {
            if (_context.Equipamentos.Any(e => e.IdentificadorBroker == identificadorUnico))
            {
                throw new InvalidOperationException("Um equipamento com este identificador já está cadastrado.");
            }

            var novoEquipamento = new Equipamento
            {
                Id = GetNextAvailableId(),
                Nome = $"Equipamento NOVO - {identificadorUnico}",
                Localizacao = "Aguardando dados iniciais do Broker",
                TipoSensor = TipoSensores.Carregando, // Um valor default, será atualizado
                Status = StatusEquipament.AguardandoDados,
                UltimaAtualizacao = DateTime.Now,
                
                // NOVO: Valor inicial para a medição.
                UltimaMedicao = "N/A - Aguardando 1ª Medição",
                
                IdentificadorBroker = identificadorUnico
            };

            _context.Equipamentos.Add(novoEquipamento);
            _context.SaveChanges();

            return novoEquipamento;
        }

        public void AtualizarDadosEmTempoReal(int id, StatusEquipament status, string localizacao, string nome, TipoSensores tipoSensor, string ultimaMedicao)
        {
            var existing = GetById(id);
            if (existing != null)
            {
                // Atualiza os dados recebidos do broker/serviço MQTT
                existing.Nome = nome;
                existing.Localizacao = localizacao;
                existing.TipoSensor = tipoSensor;
                existing.Status = status;
                existing.UltimaAtualizacao = DateTime.Now;
                
                // CRÍTICO: Atualiza o valor da medição com o dado recebido
                existing.UltimaMedicao = ultimaMedicao;

                // Salva a alteração no banco
                _context.Equipamentos.Update(existing);
                _context.SaveChanges();
            }
        }

        public void Add(Equipamento equipamento)
        {
            _context.Equipamentos.Add(equipamento);
            _context.SaveChanges();
        }

        public void Update(Equipamento equipamento)
        {
            _context.Entry(equipamento).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var equipamento = GetById(id);
            if (equipamento != null)
            {
                _context.Equipamentos.Remove(equipamento);
                _context.SaveChanges();
            }
        }

    }
}