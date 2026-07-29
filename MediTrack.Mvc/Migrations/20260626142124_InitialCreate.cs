using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MediTrack.Mvc.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Issues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IssuedTo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupplyCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MediTrack",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SupplyCategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Supplier = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    MinStock = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ConcurrencyVersion = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediTrack", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediTrack_SupplyCategories_SupplyCategoryId",
                        column: x => x.SupplyCategoryId,
                        principalTable: "SupplyCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IssueItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IssueId = table.Column<int>(type: "INTEGER", nullable: false),
                    MedicalSupplyId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueItems_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IssueItems_MediTrack_MedicalSupplyId",
                        column: x => x.MedicalSupplyId,
                        principalTable: "MediTrack",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "SupplyCategories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Bao ho" },
                    { 2, "Thiet bi kiem tra" },
                    { 3, "Tieu hao" }
                });

            migrationBuilder.InsertData(
                table: "MediTrack",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAt", "DeletedAt", "Description", "IsDeleted", "MinStock", "Name", "Quantity", "Supplier", "SupplyCategoryId", "UnitPrice", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "MS-MSK-001", 1, new DateTime(2025, 5, 15, 8, 30, 0, 0, DateTimeKind.Unspecified), null, "Khau trang y te 3 lop, phu hop cho phong kham", false, 200, "Khau trang y te", 500, "VinMed", 1, 1200m, null },
                    { 2, "MS-GLO-002", 1, new DateTime(2025, 5, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), null, "Gang tay y te khong bot, size M/L", false, 200, "Gang tay cao su", 180, "VietGlove", 1, 3400m, null },
                    { 3, "MS-THE-003", 1, new DateTime(2025, 5, 15, 10, 0, 0, 0, DateTimeKind.Unspecified), null, "Nhiet ke cam tay do nhiet do khong tiep xuc", false, 10, "Nhiet ke hong ngoai", 8, "Omron Vietnam", 2, 320000m, null },
                    { 4, "MS-BAN-004", 1, new DateTime(2025, 5, 15, 9, 30, 0, 0, DateTimeKind.Unspecified), null, "Bong y te tieu khuan, goi 500g", false, 15, "Bong y te", 0, "Medicare", 3, 28000m, null },
                    { 5, "MS-SYR-005", 1, new DateTime(2025, 5, 15, 8, 45, 0, 0, DateTimeKind.Unspecified), null, "Bom tiem 5ml 1 lan su dung", false, 100, "Bom tiem 5ml", 220, "Kim Tiem Sai Gon", 3, 1500m, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_IssueItems_IssueId",
                table: "IssueItems",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueItems_MedicalSupplyId",
                table: "IssueItems",
                column: "MedicalSupplyId");

            migrationBuilder.CreateIndex(
                name: "IX_MediTrack_Code",
                table: "MediTrack",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediTrack_SupplyCategoryId",
                table: "MediTrack",
                column: "SupplyCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IssueItems");

            migrationBuilder.DropTable(
                name: "Issues");

            migrationBuilder.DropTable(
                name: "MediTrack");

            migrationBuilder.DropTable(
                name: "SupplyCategories");
        }
    }
}
