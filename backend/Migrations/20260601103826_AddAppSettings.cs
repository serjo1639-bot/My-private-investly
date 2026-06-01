using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Investly.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAppSettings : Migration
    {
        // NOTE: This migration only creates the new `app_settings` table. EF's
        // auto-generated version also contained a large set of column renames
        // because the committed ModelSnapshot was stale (PascalCase) while the
        // live database already uses the snake_case naming convention. Those
        // renames target constraints that do not exist in the real DB, so they
        // are intentionally omitted.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "app_settings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    maintenance_mode = table.Column<bool>(type: "boolean", nullable: false),
                    maintenance_message_ar = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    maintenance_message_en = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    announcement_active = table.Column<bool>(type: "boolean", nullable: false),
                    announcement_ar = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    announcement_en = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    allow_registration = table.Column<bool>(type: "boolean", nullable: false),
                    allow_investments = table.Column<bool>(type: "boolean", nullable: false),
                    min_supported_version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_settings", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "app_settings",
                columns: new[] { "id", "allow_investments", "allow_registration", "announcement_active", "announcement_ar", "announcement_en", "maintenance_message_ar", "maintenance_message_en", "maintenance_mode", "min_supported_version", "updated_at" },
                values: new object[] { 1, true, true, false, "", "", "التطبيق قيد الصيانة حالياً. يرجى المحاولة لاحقاً.", "The app is under maintenance. Please try again later.", false, "", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_settings");
        }
    }
}
