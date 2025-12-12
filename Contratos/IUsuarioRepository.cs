// VigiLant.Contratos/IUsuarioRepository.cs

using VigiLant.Models;
using System.Threading.Tasks;
using VigiLant.Models.Enum;

public interface IUsuarioRepository
{
    Task<Usuario> BuscarPorEmail(string email);
    Task Adicionar(Usuario usuario);
    Task UpdateCargo(int usuarioId, Cargo novoCargo);
}