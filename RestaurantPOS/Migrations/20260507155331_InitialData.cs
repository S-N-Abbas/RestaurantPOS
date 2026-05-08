using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RestaurantPOS.Migrations
{
    /// <inheritdoc />
    public partial class InitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    PasscodeHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContextId = table.Column<int>(type: "INTEGER", nullable: false),
                    TableId = table.Column<int>(type: "INTEGER", nullable: true),
                    OrderType = table.Column<int>(type: "INTEGER", nullable: false),
                    AdultCovers = table.Column<int>(type: "INTEGER", nullable: false),
                    ChildCovers = table.Column<int>(type: "INTEGER", nullable: false),
                    CoverALabel = table.Column<string>(type: "TEXT", nullable: true),
                    CoverAPrice = table.Column<decimal>(type: "TEXT", nullable: true),
                    CoverBLabel = table.Column<string>(type: "TEXT", nullable: true),
                    CoverBPrice = table.Column<decimal>(type: "TEXT", nullable: true),
                    TillNo = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    ClosedBy = table.Column<string>(type: "TEXT", nullable: false),
                    IsClosed = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Tables_TableId",
                        column: x => x.TableId,
                        principalTable: "Tables",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustomerName = table.Column<string>(type: "TEXT", nullable: false),
                    CustomerPhone = table.Column<string>(type: "TEXT", nullable: false),
                    CustomerEmail = table.Column<string>(type: "TEXT", nullable: false),
                    PartySize = table.Column<int>(type: "INTEGER", nullable: false),
                    BookingDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    TableId = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DepositAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DepositPaid = table.Column<bool>(type: "INTEGER", nullable: false),
                    DepositMethod = table.Column<string>(type: "TEXT", nullable: false),
                    DepositPaidAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OrderId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Bookings_Tables_TableId",
                        column: x => x.TableId,
                        principalTable: "Tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: true),
                    ProductName = table.Column<string>(type: "TEXT", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    Method = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, true, "Starters" },
                    { 2, true, "Main Course" },
                    { 3, true, "Rice & Bread" },
                    { 4, true, "Desserts" },
                    { 5, true, "Milkshakes" },
                    { 6, true, "Mocktails" },
                    { 7, true, "Daiquiris" }
                });

            migrationBuilder.InsertData(
                table: "Tables",
                columns: new[] { "Id", "IsActive", "Number" },
                values: new object[,]
                {
                    { 1, true, 1 },
                    { 2, true, 2 },
                    { 3, true, 3 },
                    { 4, true, 4 },
                    { 5, true, 5 },
                    { 6, true, 6 },
                    { 7, true, 7 },
                    { 8, true, 8 },
                    { 9, true, 9 },
                    { 10, true, 10 },
                    { 11, true, 11 },
                    { 12, true, 12 },
                    { 13, true, 13 },
                    { 14, true, 14 },
                    { 15, true, 15 },
                    { 16, true, 16 },
                    { 17, true, 17 },
                    { 18, true, 18 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "IsActive", "PasscodeHash", "Role", "Username" },
                values: new object[] { 1, true, "1234", "Admin", "Admin" });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "IsActive", "Name", "Price" },
                values: new object[,]
                {
                    { 1, 1, true, "Samosa (3 pcs)", 3.50m },
                    { 2, 1, true, "Pakora (4 pcs)", 3.00m },
                    { 3, 1, true, "Chicken Pakora (4 pcs)", 4.00m },
                    { 4, 1, true, "Fish Pakora (4 pcs)", 4.00m },
                    { 5, 1, true, "Chapli Kabab (3 pcs)", 4.00m },
                    { 6, 1, true, "Seekh Kabab (4 pcs)", 3.50m },
                    { 7, 1, true, "Drum Stick (3 pcs)", 4.00m },
                    { 8, 1, true, "Wings (4 pcs)", 4.00m },
                    { 9, 2, true, "Dal", 6.95m },
                    { 10, 2, true, "Lamb Karahi", 8.95m },
                    { 11, 2, true, "Palak Chicken", 7.95m },
                    { 12, 2, true, "Mince Karahi", 8.95m },
                    { 13, 2, true, "Butter Chicken", 8.95m },
                    { 14, 2, true, "Chicken Manchurian", 7.95m },
                    { 15, 2, true, "Chicken Tikka Karahi", 7.95m },
                    { 16, 3, true, "Plain Rice", 5.00m },
                    { 17, 3, true, "Fried Egg Rice", 5.95m },
                    { 18, 3, true, "Pulao Rice", 7.95m },
                    { 19, 3, true, "Roti", 0.75m },
                    { 20, 3, true, "Naan", 1.50m },
                    { 21, 4, true, "Kheer", 6.95m },
                    { 22, 4, true, "Custard Trifle", 5.95m },
                    { 23, 5, true, "GoGo Shake", 4.95m },
                    { 24, 5, true, "Millionaire Shake", 4.95m },
                    { 25, 5, true, "Hershey’s Shake", 4.95m },
                    { 26, 5, true, "Reese’s Peanut Butter Shake", 4.95m },
                    { 27, 5, true, "Oreo Shake", 4.95m },
                    { 28, 5, true, "Jaffa Cake Shake", 4.95m },
                    { 29, 5, true, "Berry Nice Shake", 4.95m },
                    { 30, 6, true, "Miami Sunset", 4.95m },
                    { 31, 6, true, "Strawberry & Mint", 4.95m },
                    { 32, 6, true, "Lemon & Lime", 4.95m },
                    { 33, 6, true, "Pina Colada Mocktail", 4.95m },
                    { 34, 6, true, "Mango Mocktail", 4.95m },
                    { 35, 7, true, "Strawberry Daiquiri", 4.95m },
                    { 36, 7, true, "Mango Daiquiri", 4.95m },
                    { 37, 7, true, "Peach Daiquiri", 4.95m },
                    { 38, 7, true, "Blueberry Daiquiri", 4.95m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_OrderId",
                table: "Bookings",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TableId",
                table: "Bookings",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TableId",
                table: "Orders",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Tables");
        }
    }
}
