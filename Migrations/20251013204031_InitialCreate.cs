using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RelatoriosTI.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Relatorios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HotelNome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataGeracao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QtdLicencasAntivirus = table.Column<int>(type: "int", nullable: false),
                    QtdLicencasOffice365Standard = table.Column<int>(type: "int", nullable: false),
                    QtdLicencasOffice365Basic = table.Column<int>(type: "int", nullable: false),
                    StatusPreventivas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataRenovacaoAntivirus = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataRenovacaoOffice365 = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OutrosContratos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tickets = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CaminhoArquivoPdf = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MelhoriasProximoMes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CriadoPorUsuarioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relatorios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Senha = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EhAdministrador = table.Column<bool>(type: "bit", nullable: false),
                    HoteisPermitidos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "DataCriacao", "EhAdministrador", "Email", "HoteisPermitidos", "Senha" },
                values: new object[] { 1, new DateTime(2025, 10, 13, 17, 40, 30, 810, DateTimeKind.Local).AddTicks(9887), true, "mathaus@admin", "[]", "mathaus@123" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Relatorios");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
