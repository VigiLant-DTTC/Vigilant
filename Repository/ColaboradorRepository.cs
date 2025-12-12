using VigiLant.Contratos;
using VigiLant.Models;
using VigiLant.Data;
using Microsoft.EntityFrameworkCore;

namespace VigiLant.Repository
{
    // A classe implementa a interface ajustada
    public class ColaboradorRepository : IColaboradorRepository
    {
        private readonly BancoCtx _context;

        public ColaboradorRepository(BancoCtx context)
        {
            _context = context;
        }

        private int GetNextAvailableId()
        {
            // 1. Busca todos os IDs existentes e os ordena
            var existingIds = _context.Colaboradores
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

            return nextId;
        }

        public IEnumerable<Colaborador> GetAll()
        {
            return _context.Colaboradores.ToList();
        }

        public Colaborador? GetById(int id)
        {
            return _context.Colaboradores.Find(id);
        }

        public void Add(Colaborador colaborador)
        {
            colaborador.Id = GetNextAvailableId();
            _context.Colaboradores.Add(colaborador);
            _context.SaveChanges();
        }

        public void Update(Colaborador colaborador)
        {
            _context.Entry(colaborador).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var colaborador = GetById(id);
            if (colaborador != null)
            {
                _context.Colaboradores.Remove(colaborador);
                _context.SaveChanges();
            }
        }

        public async Task<Colaborador?> GetByEmail(string email)
        {
            return await _context.Colaboradores
                                 .FirstOrDefaultAsync(c => c.Email == email);
        }

        public void UpdateStatusVinculacao(Colaborador colaborador)
        {
            _context.Colaboradores.Update(colaborador);
            _context.SaveChanges();
        }
    }
}