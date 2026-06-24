using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HotelService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MyAmenities",
                columns: table => new
                {
                    AmenityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyAmenities", x => x.AmenityId);
                });

            migrationBuilder.CreateTable(
                name: "MyHotels",
                columns: table => new
                {
                    HotelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ZipCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ManagerId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ImageUrls = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    TotalReviews = table.Column<int>(type: "int", nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyHotels", x => x.HotelId);
                });

            migrationBuilder.CreateTable(
                name: "MyHotelAmenities",
                columns: table => new
                {
                    HotelAmenityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HotelId = table.Column<int>(type: "int", nullable: false),
                    AmenityId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyHotelAmenities", x => x.HotelAmenityId);
                    table.ForeignKey(
                        name: "FK_MyHotelAmenities_MyAmenities_AmenityId",
                        column: x => x.AmenityId,
                        principalTable: "MyAmenities",
                        principalColumn: "AmenityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MyHotelAmenities_MyHotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "MyHotels",
                        principalColumn: "HotelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MyAmenities",
                columns: new[] { "AmenityId", "Category", "CreatedAt", "Description", "Icon", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Internet", new DateTime(2026, 5, 30, 9, 10, 52, 722, DateTimeKind.Utc).AddTicks(2572), "High-speed wireless internet", "wifi", true, "Free WiFi" },
                    { 2, "Recreation", new DateTime(2026, 5, 30, 9, 10, 52, 722, DateTimeKind.Utc).AddTicks(2585), "Outdoor swimming pool", "pool", true, "Swimming Pool" },
                    { 3, "Recreation", new DateTime(2026, 5, 30, 9, 10, 52, 722, DateTimeKind.Utc).AddTicks(2588), "Fully equipped fitness center", "fitness_center", true, "Gym" },
                    { 4, "Wellness", new DateTime(2026, 5, 30, 9, 10, 52, 722, DateTimeKind.Utc).AddTicks(2592), "Full-service spa and wellness center", "spa", true, "Spa" },
                    { 5, "Dining", new DateTime(2026, 5, 30, 9, 10, 52, 722, DateTimeKind.Utc).AddTicks(2593), "On-site dining restaurant", "restaurant", true, "Restaurant" },
                    { 6, "Dining", new DateTime(2026, 5, 30, 9, 10, 52, 722, DateTimeKind.Utc).AddTicks(2595), "Lounge bar with beverages", "local_bar", true, "Bar" },
                    { 7, "Services", new DateTime(2026, 5, 30, 9, 10, 52, 722, DateTimeKind.Utc).AddTicks(2597), "Free parking available", "local_parking", true, "Parking" },
                    { 8, "Services", new DateTime(2026, 5, 30, 9, 10, 52, 722, DateTimeKind.Utc).AddTicks(2599), "24-hour room service", "room_service", true, "Room Service" },
                    { 9, "Services", new DateTime(2026, 5, 30, 9, 10, 52, 722, DateTimeKind.Utc).AddTicks(2601), "Laundry and dry cleaning services", "local_laundry_service", true, "Laundry" },
                    { 10, "Transport", new DateTime(2026, 5, 30, 9, 10, 52, 722, DateTimeKind.Utc).AddTicks(2602), "Airport pickup and drop service", "airport_shuttle", true, "Airport Shuttle" },
                    { 11, "Business", new DateTime(2026, 5, 30, 9, 10, 52, 722, DateTimeKind.Utc).AddTicks(2604), "Business center with meeting rooms", "business_center", true, "Business Center" },
                    { 12, "Business", new DateTime(2026, 5, 30, 9, 10, 52, 722, DateTimeKind.Utc).AddTicks(2606), "Conference and event facilities", "meeting_room", true, "Conference Room" },
                    { 13, "Services", new DateTime(2026, 5, 30, 9, 10, 52, 722, DateTimeKind.Utc).AddTicks(2607), "Pets are allowed", "pets", true, "Pet Friendly" },
                    { 14, "Room", new DateTime(2026, 5, 30, 9, 10, 52, 722, DateTimeKind.Utc).AddTicks(2609), "Climate controlled rooms", "ac_unit", true, "Air Conditioning" },
                    { 15, "Services", new DateTime(2026, 5, 30, 9, 10, 52, 722, DateTimeKind.Utc).AddTicks(2611), "Round the clock reception", "support_agent", true, "24/7 Front Desk" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MyHotelAmenities_AmenityId",
                table: "MyHotelAmenities",
                column: "AmenityId");

            migrationBuilder.CreateIndex(
                name: "IX_MyHotelAmenities_HotelId",
                table: "MyHotelAmenities",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_MyHotels_City",
                table: "MyHotels",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_MyHotels_Name",
                table: "MyHotels",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MyHotelAmenities");

            migrationBuilder.DropTable(
                name: "MyAmenities");

            migrationBuilder.DropTable(
                name: "MyHotels");
        }
    }
}
