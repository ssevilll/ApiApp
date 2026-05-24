using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class mig_4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "ffa57908-6454-44be-9ebc-390365ed7add", "ef36482b-4f4a-4353-b721-9526d5175b00" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "c4311c44-09b9-4edc-93be-c35df9cd780c", "a6b156ff-0ad0-4ee4-999b-ed2d9011c540" });
        }
    }
}
