using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RelatoriosTI.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Senha = table.Column<string>(type: "TEXT", nullable: false),
                    EhAdministrador = table.Column<bool>(type: "INTEGER", nullable: false),
                    HoteisPermitidos = table.Column<string>(type: "TEXT", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Relatorios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HotelNome = table.Column<string>(type: "TEXT", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataGeracao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    QtdLicencasAntivirus = table.Column<int>(type: "INTEGER", nullable: false),
                    QtdLicencasOffice365Standard = table.Column<int>(type: "INTEGER", nullable: false),
                    QtdLicencasOffice365Basic = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusPreventivas = table.Column<string>(type: "TEXT", nullable: false),
                    DataRenovacaoAntivirus = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataRenovacaoOffice365 = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OutrosContratos = table.Column<string>(type: "TEXT", nullable: true),
                    Tickets = table.Column<string>(type: "TEXT", nullable: false),
                    CaminhoArquivoPdf = table.Column<string>(type: "TEXT", nullable: true),
                    MelhoriasProximoMes = table.Column<string>(type: "TEXT", nullable: false),
                    MelhoriasFeitas = table.Column<string>(type: "TEXT", nullable: false),
                    MelhoriasFeitasConfirmado = table.Column<bool>(type: "INTEGER", nullable: false),
                    ResumoMes = table.Column<string>(type: "TEXT", nullable: false),
                    QtdMaquinasTotal = table.Column<int>(type: "INTEGER", nullable: false),
                    QtdMaquinasPreventivasFeitas = table.Column<int>(type: "INTEGER", nullable: false),
                    MotivoPreventivasIncompletas = table.Column<string>(type: "TEXT", nullable: false),
                    AssinaturaUsuarioNome = table.Column<string>(type: "TEXT", nullable: false),
                    DataAssinatura = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RelatorioAprovado = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoPorUsuarioId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relatorios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Relatorios_Usuarios_CriadoPorUsuarioId",
                        column: x => x.CriadoPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "DataCriacao", "EhAdministrador", "Email", "HoteisPermitidos", "Senha" },
                values: new object[] { 1, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "mathaus@admin", "[]", "$2a$11$v2YsLtwHrb72xNnvQ4gzoOY527yzeM3XcOSRS.L018TXSHTUjTIGa" });

            migrationBuilder.CreateIndex(
                name: "IX_Relatorios_CriadoPorUsuarioId",
                table: "Relatorios",
                column: "CriadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Relatorios_DataCriacao",
                table: "Relatorios",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_Relatorios_HotelNome",
                table: "Relatorios",
                column: "HotelNome");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
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
