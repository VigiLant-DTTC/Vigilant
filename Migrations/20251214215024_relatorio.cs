using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VigiLant.Migrations
{
    /// <inheritdoc />
    public partial class relatorio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Relatorios_Equipamentos_EquipamentoId",
                table: "Relatorios");

            migrationBuilder.DropIndex(
                name: "IX_Relatorios_EquipamentoId",
                table: "Relatorios");

            migrationBuilder.DropColumn(
                name: "EquipamentoId",
                table: "Relatorios");

            migrationBuilder.AddColumn<string>(
                name: "EquipamentoNome",
                table: "Relatorios",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EquipamentoNome",
                table: "Relatorios");

            migrationBuilder.AddColumn<int>(
                name: "EquipamentoId",
                table: "Relatorios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Relatorios_EquipamentoId",
                table: "Relatorios",
                column: "EquipamentoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Relatorios_Equipamentos_EquipamentoId",
                table: "Relatorios",
                column: "EquipamentoId",
                principalTable: "Equipamentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
