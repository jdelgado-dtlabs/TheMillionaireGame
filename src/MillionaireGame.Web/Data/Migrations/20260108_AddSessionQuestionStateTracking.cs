using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MillionaireGame.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionQuestionStateTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentQuestionText",
                table: "Sessions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentQuestionOptionsJson",
                table: "Sessions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentQuestionText",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "CurrentQuestionOptionsJson",
                table: "Sessions");
        }
    }
}
