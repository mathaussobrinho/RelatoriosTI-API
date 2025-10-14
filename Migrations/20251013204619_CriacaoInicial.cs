using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RelatoriosTI.API.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataCriacao",
                value: new DateTime(2025, 10, 13, 17, 46, 16, 101, DateTimeKind.Local).AddTicks(7818));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataCriacao",
                value: new DateTime(2025, 10, 13, 17, 40, 30, 810, DateTimeKind.Local).AddTicks(9887));
        }
    }
}
