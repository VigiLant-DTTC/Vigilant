using VigiLant.Models;

namespace VigiLant.Contratos
{
    public interface IColaboradorRepository
    {
        IEnumerable<Colaborador> GetAll();

        Colaborador? GetById(int id);

        void Add(Colaborador colaborador);

        void Update(Colaborador colaborador);

        void Delete(int id);

        Task<Colaborador?> GetByEmail(string email);
        void UpdateStatusVinculacao(Colaborador colaborador);
    }
}