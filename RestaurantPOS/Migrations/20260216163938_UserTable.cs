using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RestaurantPOS.Migrations
{
    /// <inheritdoc />
    public partial class UserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "IsActive", "PasscodeHash", "Role", "Username" },
                values: new object[,]
                {
                    { 1, true, "$2a$11$vl0UxJaGDJQLPFSER5kYE.nMVmYXM4jqWLU7zHVYEAHU9IC2hMhq.", 1, "Abbas" },
                    { 2, true, "$2a$11$3E1HMTpo7ekBujQ2zXTWtOTY.6nVLsbqGSkX/.O3tlZ5fdl4uZXui", 2, "Bilal" }
                });
        }
    }
}
