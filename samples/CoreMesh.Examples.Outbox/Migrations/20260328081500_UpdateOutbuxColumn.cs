using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreMesh.Examples.Outbox.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOutbuxColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Headers",
                table: "outbox_messages",
                newName: "Header");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Header",
                table: "outbox_messages",
                newName: "Headers");
        }
    }
}
