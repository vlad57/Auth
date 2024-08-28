using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_Custom.Migrations
{
    /// <inheritdoc />
    public partial class Added_Phone_Code : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PhoneCode",
                table: "Users",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneCode",
                table: "Users");
        }
    }
}
