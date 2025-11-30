using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyBuddy.Migrations
{
    /// <inheritdoc />
    public partial class addedStudyTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_AspNetUsers_UserId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Subjects_SubjectId",
                table: "Tasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks");

            migrationBuilder.RenameTable(
                name: "Tasks",
                newName: "StudyTasks");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_UserId",
                table: "StudyTasks",
                newName: "IX_StudyTasks_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_SubjectId",
                table: "StudyTasks",
                newName: "IX_StudyTasks_SubjectId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudyTasks",
                table: "StudyTasks",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudyTasks_AspNetUsers_UserId",
                table: "StudyTasks",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
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
                name: "FK_StudyTasks_AspNetUsers_UserId",
                table: "StudyTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_StudyTasks_Subjects_SubjectId",
                table: "StudyTasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudyTasks",
                table: "StudyTasks");

            migrationBuilder.RenameTable(
                name: "StudyTasks",
                newName: "Tasks");

            migrationBuilder.RenameIndex(
                name: "IX_StudyTasks_UserId",
                table: "Tasks",
                newName: "IX_Tasks_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_StudyTasks_SubjectId",
                table: "Tasks",
                newName: "IX_Tasks_SubjectId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_AspNetUsers_UserId",
                table: "Tasks",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Subjects_SubjectId",
                table: "Tasks",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "SubjectId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
