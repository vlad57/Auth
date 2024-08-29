using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_Custom.Migrations
{
    /// <inheritdoc />
    public partial class Add_EmailCode_to_user : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmailCode",
                table: "Users",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailCode",
                table: "Users");
        }
    }
}
