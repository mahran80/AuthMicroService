using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Users.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialUsersSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "users");

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                schema: "users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.UserId);
                });

            migrationBuilder.CreateIndex(
                name: "UX_UserProfiles_Email",
                schema: "users",
                table: "UserProfiles",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_UserProfiles_Phone",
                schema: "users",
                table: "UserProfiles",
                columns: new[] { "CountryCode", "PhoneNumber" },
                unique: true,
                filter: "[PhoneNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserProfiles",
                schema: "users");
        }
    }
}
