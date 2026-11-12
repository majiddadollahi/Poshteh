using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PM.Identities.Infra.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Identities_AccessPanelAndRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Panel",
                schema: "Identity",
                table: "Accesses",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "Requirements",
                schema: "Identity",
                table: "Accesses",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Accesses_Panel",
                schema: "Identity",
                table: "Accesses",
                sql: "[Panel] BETWEEN 0 AND 6");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Accesses_Requirements",
                schema: "Identity",
                table: "Accesses",
                sql: "[Requirements] BETWEEN 0 AND 3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Accesses_Panel",
                schema: "Identity",
                table: "Accesses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Accesses_Requirements",
                schema: "Identity",
                table: "Accesses");

            migrationBuilder.DropColumn(
                name: "Panel",
                schema: "Identity",
                table: "Accesses");

            migrationBuilder.DropColumn(
                name: "Requirements",
                schema: "Identity",
                table: "Accesses");
        }
    }
}
