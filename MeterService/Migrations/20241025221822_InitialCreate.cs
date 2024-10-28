using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeterService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MeterReadings",
                columns: table => new
                {
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    SerialNumber = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    ReadingTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastIndex = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VoltageValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeterReadings", x => x.UUID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeterReadings");
        }
    }
}
