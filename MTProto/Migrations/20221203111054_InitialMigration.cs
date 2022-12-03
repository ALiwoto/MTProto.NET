using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MTProto.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OwnerInfos",
                columns: table => new
                {
                    OwnerId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IsBot = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnerInfos", x => x.OwnerId);
                });

            migrationBuilder.CreateTable(
                name: "PeerInfos",
                columns: table => new
                {
                    PeerId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccessHash = table.Column<string>(type: "TEXT", nullable: true),
                    PeerType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeerInfos", x => x.PeerId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OwnerInfos");

            migrationBuilder.DropTable(
                name: "PeerInfos");
        }
    }
}
