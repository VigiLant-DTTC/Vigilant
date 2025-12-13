using VigiLant.Models;
using System.Collections.Generic;

namespace VigiLant.Contratos
{
    public interface IAnaliseRepository
    {
        IEnumerable<Analise> GetAll();

        Analise GetById(int id);

        void Add(Analise analise);
        void Delete(int id);
    }
}