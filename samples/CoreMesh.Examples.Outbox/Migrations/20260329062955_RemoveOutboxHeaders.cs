using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreMesh.Examples.Outbox.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOutboxHeaders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Header",
                table: "outbox_messages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Header",
                table: "outbox_messages",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
