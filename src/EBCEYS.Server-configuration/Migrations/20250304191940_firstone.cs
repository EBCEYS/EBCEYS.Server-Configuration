using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EBCEYS.Server_configuration.Migrations
{
    /// <inheritdoc />
    public partial class Firstone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "containers",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    configuration_path = table.Column<string>(type: "TEXT", nullable: false),
                    last_config_set_UTC = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    is_exists = table.Column<bool>(type: "INTEGER", nullable: false),
                    marked_for_deletion_UTC = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_containers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "configuration_files",
                columns: table => new
                {
                    file_path = table.Column<string>(type: "TEXT", nullable: false),
                    container_id = table.Column<string>(type: "TEXT", nullable: false),
                    container_file_path = table.Column<string>(type: "TEXT", nullable: false),
                    file_m_time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    file_last_update = table.Column<DateTime>(type: "TEXT", nullable: false),
                    is_exists = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuration_files", x => x.file_path);
                    table.ForeignKey(
                        name: "FK_configuration_files_containers_container_id",
                        column: x => x.container_id,
                        principalTable: "containers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_configuration_files_container_id",
                table: "configuration_files",
                column: "container_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "configuration_files");

            migrationBuilder.DropTable(
                name: "containers");
        }
    }
}
