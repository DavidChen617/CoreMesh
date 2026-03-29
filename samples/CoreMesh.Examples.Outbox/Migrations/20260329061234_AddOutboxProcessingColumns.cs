using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreMesh.Examples.Outbox.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxProcessingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClaimId",
                table: "outbox_messages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessingStartedAt",
                table: "outbox_messages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_ClaimId",
                table: "outbox_messages",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_Status_ProcessingStartedAt",
                table: "outbox_messages",
                columns: new[] { "Status", "ProcessingStartedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_outbox_messages_ClaimId",
                table: "outbox_messages");

            migrationBuilder.DropIndex(
                name: "IX_outbox_messages_Status_ProcessingStartedAt",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "ClaimId",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "ProcessingStartedAt",
                table: "outbox_messages");
        }
    }
}
