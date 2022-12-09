using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MTProto.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "AuthKey",
                table: "OwnerInfos",
                type: "BLOB",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthKey",
                table: "OwnerInfos");
        }
    }
}
