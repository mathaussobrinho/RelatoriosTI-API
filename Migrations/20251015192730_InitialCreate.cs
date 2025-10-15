using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RelatoriosTI.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "Usuarios",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Senha = table.Column<string>(type: "text", nullable: false),
                    EhAdministrador = table.Column<bool>(type: "boolean", nullable: false),
                    HoteisPermitidos = table.Column<string>(type: "jsonb", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Relatorios",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HotelNome = table.Column<string>(type: "text", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataGeracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    QtdLicencasAntivirus = table.Column<int>(type: "integer", nullable: false),
                    QtdLicencasOffice365Standard = table.Column<int>(type: "integer", nullable: false),
                    QtdLicencasOffice365Basic = table.Column<int>(type: "integer", nullable: false),
                    StatusPreventivas = table.Column<string>(type: "text", nullable: false),
                    DataRenovacaoAntivirus = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataRenovacaoOffice365 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OutrosContratos = table.Column<string>(type: "text", nullable: true),
                    Tickets = table.Column<string>(type: "jsonb", nullable: false),
                    CaminhoArquivoPdf = table.Column<string>(type: "text", nullable: true),
                    MelhoriasProximoMes = table.Column<string>(type: "text", nullable: false),
                    CriadoPorUsuarioId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relatorios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Relatorios_Usuarios_CriadoPorUsuarioId",
                        column: x => x.CriadoPorUsuarioId,
                        principalSchema: "public",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "Usuarios",
                columns: new[] { "Id", "DataCriacao", "EhAdministrador", "Email", "HoteisPermitidos", "Senha" },
                values: new object[] { 1, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "mathaus@admin", "[]", "$2a$11$JSTxO/0C7YH34nJvss1xqOl.zwCvzLB9r9ajbI5wLAyz5LiY9Jp4y" });

            migrationBuilder.CreateIndex(
                name: "IX_Relatorios_CriadoPorUsuarioId",
                schema: "public",
                table: "Relatorios",
                column: "CriadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Relatorios_DataCriacao",
                schema: "public",
                table: "Relatorios",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_Relatorios_HotelNome",
                schema: "public",
                table: "Relatorios",
                column: "HotelNome");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                schema: "public",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Relatorios",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Usuarios",
                schema: "public");
        }
    }
}
