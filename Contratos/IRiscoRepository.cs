using VigiLant.Models;

namespace VigiLant.Contratos
{
    public interface IRiscoRepository
    {
        IEnumerable<Risco> GetAll();
        IEnumerable<VigiLant.Models.DTO.RiscoMensalDTO> GetRiscosPorMesNoAno(int ano);

        Risco GetById(int id);

        void Add(Risco risco);

        void Update(Risco risco);

        void Delete(int id);
    }
}