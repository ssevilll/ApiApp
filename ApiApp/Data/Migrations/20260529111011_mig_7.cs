using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class mig_7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7c610a56-d6be-418a-9d6a-7b7fe103cf24", "ADMIN@EVENTAPP.COM", "ADMIN@EVENTAPP.COM", "AQAAAAIAAYagAAAAEBB5LdAT2k/leuLpkZnyEq4CJwVy2FSTZpR3p++FN8UXExDBVE5LnlcucQW8iz9toQ==", "98b746d7-aa8b-4386-9f36-5008f3fbb0f5" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d9f2313c-ce1d-415f-98e9-ff6504e00305", "ADMİN@EVENTAPP.COM", "ADMİN@EVENTAPP.COM", "AQAAAAIAAYagAAAAEBU5iFfJQLCHjNgW6eTXKS1Iu3OpnREjhnc2rZXMmgLygD2Bpi1v1yqIiqnYUw/k9w==", "0a5729b7-0f97-43df-ba39-99194a83448e" });
        }
    }
}
