using VigiLant.Contratos;
using VigiLant.Models;
using VigiLant.Data; // Assumindo que você usa este namespace
using Microsoft.EntityFrameworkCore;

namespace VigiLant.Repository
{
    public class AnaliseRepository : IAnaliseRepository
    {
        private readonly BancoCtx _context;

        public AnaliseRepository(BancoCtx context)
        {
            _context = context;
        }

        // Simplesmente obtém o próximo ID sequencial
        private int GetNextAvailableId()
        {
            if (!_context.Analises.Any()) return 1;
            return _context.Analises.Max(a => a.Id) + 1;
        }
        
        public IEnumerable<Analise> GetAll()
        {
            return _context.Analises.Include(a => a.Risco).ToList();
        }

        public Analise GetById(int id)
        {
            return _context.Analises.Include(a => a.Risco).FirstOrDefault(a => a.Id == id);
        }

        public void Add(Analise analise)
        {
            analise.Id = GetNextAvailableId(); 
            _context.Analises.Add(analise);
            _context.SaveChanges(); 
        }

        public void Delete(int id)
        {
            var analise = GetById(id);
            if (analise != null)
            {
                _context.Analises.Remove(analise);
                _context.SaveChanges(); 
            }
        }
    }
}