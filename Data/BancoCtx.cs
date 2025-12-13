using Microsoft.EntityFrameworkCore;
using VigiLant.Models;
using VigiLant.Models.Enum;

namespace VigiLant.Data
{
    public class BancoCtx : DbContext
    {
        public BancoCtx(DbContextOptions<BancoCtx> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Risco>()
                .Property(r => r.NivelGravidade)
                .HasConversion<string>();
        }

        public DbSet<Colaborador> Colaboradores { get; set; }
        public DbSet<Equipamento> Equipamentos { get; set; }
        public DbSet<Risco> Riscos { get; set; }
        public DbSet<Relatorio> Relatorios { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Analise> Analises { get; set; }


        public DbSet<AppConfig> AppConfigs { get; set; }
    }
}