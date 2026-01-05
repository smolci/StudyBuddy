using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyBuddy.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectTopicsCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Topics_SubjectId",
                table: "Topics");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Topics",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_SubjectId_Name",
                table: "Topics",
                columns: new[] { "SubjectId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Topics_SubjectId_Name",
                table: "Topics");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Topics",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_SubjectId",
                table: "Topics",
                column: "SubjectId");
        }
    }
}
