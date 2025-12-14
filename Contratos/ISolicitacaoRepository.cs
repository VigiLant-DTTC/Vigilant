using System.Collections.Generic;
using System.Threading.Tasks;
using VigiLant.Models;
using VigiLant.Models.Enum;

namespace VigiLant.Contratos
{
    public interface ISolicitacaoRepository
    {
        Task Adicionar(Solicitacao solicitacao);
        Solicitacao GetById(int id);
        IEnumerable<Solicitacao> GetAllPendentes();
        void UpdateStatus(int id, StatusSolicitacao novoStatus);
    }
}