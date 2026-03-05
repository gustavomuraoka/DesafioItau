using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompraAutomatizada.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClienteIdToDistribuicao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ClienteId",
                table: "Distribuicoes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "Distribuicoes");
        }
    }
}
