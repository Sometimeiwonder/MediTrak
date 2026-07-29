using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediTrack.Mvc.Migrations
{
    /// <inheritdoc />
    public partial class Lab06FixDiacritics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MediTrack",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Kh?u trang y t? 3 l?p, phù h?p cho ph?ng khám", "Kh?u trang y t?" });

            migrationBuilder.UpdateData(
                table: "MediTrack",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Gãng tay y t? không b?t, size M/L", "Gãng tay cao su" });

            migrationBuilder.UpdateData(
                table: "MediTrack",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Nhi?t k? c?m tay ðo nhi?t ð? không ti?p xúc", "Nhi?t k? h?ng ngo?i" });

            migrationBuilder.UpdateData(
                table: "MediTrack",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Bông y t? tiêu khu?n, gói 500g", "Bông y t?" });

            migrationBuilder.UpdateData(
                table: "MediTrack",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Name", "Supplier" },
                values: new object[] { "Bõm tiêm 5ml 1 l?n s? d?ng", "Bõm tiêm 5ml", "Kim Tiêm Sài G?n" });

            migrationBuilder.UpdateData(
                table: "SupplyCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "B?o h?");

            migrationBuilder.UpdateData(
                table: "SupplyCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Thi?t b? ki?m tra");

            migrationBuilder.UpdateData(
                table: "SupplyCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Tiêu hao");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MediTrack",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Khau trang y te 3 lop, phu hop cho phong kham", "Khau trang y te" });

            migrationBuilder.UpdateData(
                table: "MediTrack",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Gang tay y te khong bot, size M/L", "Gang tay cao su" });

            migrationBuilder.UpdateData(
                table: "MediTrack",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Nhiet ke cam tay do nhiet do khong tiep xuc", "Nhiet ke hong ngoai" });

            migrationBuilder.UpdateData(
                table: "MediTrack",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Bong y te tieu khuan, goi 500g", "Bong y te" });

            migrationBuilder.UpdateData(
                table: "MediTrack",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Name", "Supplier" },
                values: new object[] { "Bom tiem 5ml 1 lan su dung", "Bom tiem 5ml", "Kim Tiem Sai Gon" });

            migrationBuilder.UpdateData(
                table: "SupplyCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Bao ho");

            migrationBuilder.UpdateData(
                table: "SupplyCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Thiet bi kiem tra");

            migrationBuilder.UpdateData(
                table: "SupplyCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Tieu hao");
        }
    }
}
