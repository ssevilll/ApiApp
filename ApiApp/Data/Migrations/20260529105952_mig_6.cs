using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class mig_6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d9f2313c-ce1d-415f-98e9-ff6504e00305", "AQAAAAIAAYagAAAAEBU5iFfJQLCHjNgW6eTXKS1Iu3OpnREjhnc2rZXMmgLygD2Bpi1v1yqIiqnYUw/k9w==", "0a5729b7-0f97-43df-ba39-99194a83448e" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "259f372a-bb16-4db7-887b-4831afaf14da", "AQAAAAIAAYagAAAAEO3Q84A77gmXrOfRKfTGjZWrjxdct428+acN7kNzleHGogV4NxeCF8lCjP8kPn79MA==", "3141ee7b-d72a-427e-8492-af40cf9f0904" });
        }
    }
}
