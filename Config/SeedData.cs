using Microsoft.Extensions.DependencyInjection;
using VigiLant.Contratos;
using VigiLant.Data;
using VigiLant.Models;
using VigiLant.Models.Enum;
using VigiLant.Services; 

namespace VigiLant.Config
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<BancoCtx>();
                var hashService = scope.ServiceProvider.GetRequiredService<IHashService>();

                // 1. Garante que o banco de dados está criado (e executa Migrations)
                await context.Database.EnsureCreatedAsync();

                // 2. Cria o Administrador de Testes APENAS SE ELE NÃO EXISTIR
                string emailAdmin = "admin@vigilant.com.br";

                if (!context.Usuarios.Any(u => u.Email == emailAdmin))
                {
                    // Dados do Administrador
                    var adminUsuario = new Usuario
                    {
                        Nome = "Adm",
                        Email = emailAdmin,
                        // Usar o Hash Service para a senha (ex: "Admin123!")
                        SenhaHash = hashService.GerarHash("Admin123"),
                        cargo = Cargo.Administrador,
                    };

                    // Adiciona e salva o usuário
                    context.Usuarios.Add(adminUsuario);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
