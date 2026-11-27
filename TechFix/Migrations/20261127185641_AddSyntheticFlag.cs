using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechFix.Migrations
{
    /// <inheritdoc />
    public partial class AddSyntheticFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSynthetic",
                table: "Appointments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSynthetic",
                table: "Appointments");
        }
    }
}
