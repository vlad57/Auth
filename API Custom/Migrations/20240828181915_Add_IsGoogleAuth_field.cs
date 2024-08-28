using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_Custom.Migrations
{
    /// <inheritdoc />
    public partial class Add_IsGoogleAuth_field : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsGoogleAuth",
                table: "Users",
                type: "bit",
                nullable: true,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGoogleAuth",
                table: "Users");
        }
    }
}
