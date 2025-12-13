namespace VigiLant.Contratos
{
    public interface IIAService
    {
        Task<Tuple<string, string>> GerarAnaliseDeRisco(string nomeRisco, string descricaoRisco);
    }
}