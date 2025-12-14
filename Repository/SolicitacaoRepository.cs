using VigiLant.Contratos;
using VigiLant.Models;
using VigiLant.Models.Enum;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VigiLant.Data; // Assumindo EF Core

namespace VigiLant.Repository
{
    // Assumindo que você tem um ApplicationDbContext
    public class SolicitacaoRepository : ISolicitacaoRepository
    {
        private readonly BancoCtx _context;

        public SolicitacaoRepository(BancoCtx context) 
        {
            _context = context;
        }

        public async Task Adicionar(Solicitacao solicitacao)
        {
            await _context.Solicitacoes.AddAsync(solicitacao);
            await _context.SaveChangesAsync();
        }

        public Solicitacao GetById(int id)
        {
            // Usando Find ou FirstOrDefault
            return _context.Solicitacoes.FirstOrDefault(s => s.Id == id);
        }

        public IEnumerable<Solicitacao> GetAllPendentes()
        {
            return _context.Solicitacoes.Where(s => s.Status == StatusSolicitacao.Pendente).ToList();
        }

        public void UpdateStatus(int id, StatusSolicitacao novoStatus)
        {
            var solicitacao = _context.Solicitacoes.FirstOrDefault(s => s.Id == id);
            if (solicitacao != null)
            {
                solicitacao.Status = novoStatus;
                _context.Solicitacoes.Update(solicitacao);
                _context.SaveChanges();
            }
        }
    }
}