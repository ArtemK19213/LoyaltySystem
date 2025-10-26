using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoyaltySystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddLoyaltyDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CardNumberCounters",
                columns: table => new
                {
                    OrgId = table.Column<int>(type: "int", nullable: false),
                    ProgramId = table.Column<int>(type: "int", nullable: false),
                    NextSeq = table.Column<long>(type: "bigint", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardNumberCounters", x => new { x.OrgId, x.ProgramId });
                });

            migrationBuilder.CreateTable(
                name: "Programs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrgId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProgramType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PointsPerCurrency = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RoundingMode = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    MinOrderTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxPointsPerOrder = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DailyEarnLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RedeemStep = table.Column<int>(type: "int", nullable: true),
                    ExpireMonths = table.Column<int>(type: "int", nullable: true),
                    BaseDiscountPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ThemeColorStart = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: true),
                    ThemeColorEnd = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: true),
                    CardPrefix = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Programs_Orgs_OrgId",
                        column: x => x.OrgId,
                        principalTable: "Orgs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrgId = table.Column<int>(type: "int", nullable: false),
                    ProgramId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    PublicNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    QrSecret = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false, defaultValue: "Active"),
                    Tier = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cards_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cards_Users_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgramTiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramId = table.Column<int>(type: "int", nullable: false),
                    ThresholdAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramTiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramTiers_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ledger",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrgId = table.Column<int>(type: "int", nullable: false),
                    ProgramId = table.Column<int>(type: "int", nullable: false),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    Points = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PerformedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ledger", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ledger_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_ClientId",
                table: "Cards",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_OrgId_ProgramId_ClientId",
                table: "Cards",
                columns: new[] { "OrgId", "ProgramId", "ClientId" });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_ProgramId",
                table: "Cards",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_PublicNumber",
                table: "Cards",
                column: "PublicNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cards_QrSecret",
                table: "Cards",
                column: "QrSecret",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ledger_CardId_CreatedAt",
                table: "Ledger",
                columns: new[] { "CardId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Ledger_OrgId_IdempotencyKey",
                table: "Ledger",
                columns: new[] { "OrgId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ledger_OrgId_OrderId",
                table: "Ledger",
                columns: new[] { "OrgId", "OrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_Programs_OrgId_Name",
                table: "Programs",
                columns: new[] { "OrgId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgramTiers_ProgramId",
                table: "ProgramTiers",
                column: "ProgramId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardNumberCounters");

            migrationBuilder.DropTable(
                name: "Ledger");

            migrationBuilder.DropTable(
                name: "ProgramTiers");

            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "Programs");
        }
    }
}
