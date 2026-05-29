using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class mig_5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "SecurityStamp" },
                values: new object[] { "259f372a-bb16-4db7-887b-4831afaf14da", "ADMİN@EVENTAPP.COM", "ADMİN@EVENTAPP.COM", "AQAAAAIAAYagAAAAEO3Q84A77gmXrOfRKfTGjZWrjxdct428+acN7kNzleHGogV4NxeCF8lCjP8kPn79MA==", "3141ee7b-d72a-427e-8492-af40cf9f0904" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ffa57908-6454-44be-9ebc-390365ed7add", "ADMIN@EVENTAPP.COM", "ADMIN@EVENTAPP.COM", "<PASTE_HASHED_PASSWORD_HERE>", "ef36482b-4f4a-4353-b721-9526d5175b00" });
        }
    }
}
