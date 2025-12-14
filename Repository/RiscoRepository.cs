using VigiLant.Contratos;
using VigiLant.Models;
using VigiLant.Data; // Adicione este using para o seu DbContext
using Microsoft.EntityFrameworkCore; // Para usar métodos do EF Core
using System.Linq;
using VigiLant.Models.DTO; // Para usar métodos LINQ (OrderBy, Select)

namespace VigiLant.Repository
{
    public class RiscoRepository : IRiscoRepository
    {
        private readonly BancoCtx _context;

        public RiscoRepository(BancoCtx context)
        {
            _context = context;
        }

        private int GetNextAvailableId()
        {
            // 1. Busca todos os IDs existentes e os ordena
            var existingIds = _context.Riscos
                                    .Select(r => r.Id)
                                    .OrderBy(id => id)
                                    .ToList();

            if (!existingIds.Any())
            {
                return 1; // Tabela vazia, começa em 1
            }

            int nextId = 1;
            
            // 2. Itera para encontrar o primeiro ID faltante (o buraco)
            foreach (var id in existingIds)
            {
                if (id > nextId)
                {
                    // Ex: IDs são {1, 3, 4}. nextId é 2, id é 3. Retorna 2.
                    return nextId; 
                }
                nextId = id + 1; // Move para o próximo ID sequencial
            }

            // 3. Se não houver buracos (Ex: 1, 2, 3), retorna o próximo sequencial
            return nextId;
        }
        
        public IEnumerable<RiscoMensalDTO> GetRiscosPorMesNoAno(int ano)
        {
            // Filtra os riscos pelo ano atual e agrupa pela propriedade Month
            return _context.Riscos
                .Where(r => r.DataIdentificacao.Year == ano)
                .GroupBy(r => r.DataIdentificacao.Month)
                .Select(g => new RiscoMensalDTO
                {
                    Mes = g.Key,
                    Contagem = g.Count()
                })
                .OrderBy(d => d.Mes)
                .ToList();
        }

        public IEnumerable<Risco> GetAll()
        {
            return _context.Riscos.ToList();
        }

        public Risco GetById(int id)
        {
            return _context.Riscos.Find(id);
        }

        public void Add(Risco risco)
        {
            risco.Id = GetNextAvailableId(); 
            _context.Riscos.Add(risco);
            _context.SaveChanges(); 
        }

        public void Update(Risco risco)
        {
            _context.Entry(risco).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var risco = GetById(id);
            if (risco != null)
            {
                _context.Riscos.Remove(risco);
                // O SaveChanges aqui cria o buraco que será preenchido no próximo Add
                _context.SaveChanges(); 
            }
        }
    }
}