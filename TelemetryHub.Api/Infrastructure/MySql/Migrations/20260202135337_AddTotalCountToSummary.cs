using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelemetryHub.Api.Infrastructure.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalCountToSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalCount",
                table: "TelemetrySummaries",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalCount",
                table: "TelemetrySummaries");
        }
    }
}
