using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyBuddy.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeleteSubjectRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudySessions_Subjects_SubjectId",
                table: "StudySessions");

            migrationBuilder.DropForeignKey(
                name: "FK_StudyTasks_Subjects_SubjectId",
                table: "StudyTasks");

            migrationBuilder.AddForeignKey(
                name: "FK_StudySessions_Subjects_SubjectId",
                table: "StudySessions",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "SubjectId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudyTasks_Subjects_SubjectId",
                table: "StudyTasks",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "SubjectId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudySessions_Subjects_SubjectId",
                table: "StudySessions");

            migrationBuilder.DropForeignKey(
                name: "FK_StudyTasks_Subjects_SubjectId",
                table: "StudyTasks");

            migrationBuilder.AddForeignKey(
                name: "FK_StudySessions_Subjects_SubjectId",
                table: "StudySessions",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "SubjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudyTasks_Subjects_SubjectId",
                table: "StudyTasks",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "SubjectId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
