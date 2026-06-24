using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RoomService.Migrations
{
    /// <inheritdoc />
    public partial class AddAmenityMasterTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MyRoomAmenities_RoomId",
                table: "MyRoomAmenities");

            migrationBuilder.DropColumn(
                name: "AmenityName",
                table: "MyRoomAmenities");

            migrationBuilder.CreateTable(
                name: "MyRoomAmenitiesMaster",
                columns: table => new
                {
                    AmenityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyRoomAmenitiesMaster", x => x.AmenityId);
                });

            migrationBuilder.InsertData(
                table: "MyRoomAmenitiesMaster",
                columns: new[] { "AmenityId", "Category", "Description", "Icon", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Climate", "Climate controlled room temperature", "ac_unit", true, "Air Conditioning" },
                    { 2, "Climate", "Central heating system", "thermostat", true, "Heating" },
                    { 3, "Food & Drink", "In-room mini refrigerator with beverages", "local_bar", true, "Mini Bar" },
                    { 4, "Food & Drink", "In-room coffee and tea maker", "coffee_maker", true, "Coffee Maker" },
                    { 5, "Food & Drink", "24-hour room service available", "room_service", true, "Room Service" },
                    { 6, "Entertainment", "LED/LCD flat screen television", "tv", true, "Flat Screen TV" },
                    { 7, "Entertainment", "Premium cable channels", "live_tv", true, "Cable/Satellite TV" },
                    { 8, "Technology", "High-speed wireless internet", "wifi", true, "Free WiFi" },
                    { 9, "Business", "Dedicated workspace with chair", "desk", true, "Work Desk" },
                    { 10, "Security", "In-room electronic safe", "lock", true, "Safe Box" },
                    { 11, "Bathroom", "Private bathtub", "bathtub", true, "Bathtub" },
                    { 12, "Bathroom", "Rainfall showerhead", "shower", true, "Rain Shower" },
                    { 13, "Bathroom", "In-room hair dryer", "dry", true, "Hair Dryer" },
                    { 14, "Bathroom", "Complimentary bathroom amenities", "soap", true, "Toiletries" },
                    { 15, "Outdoor", "Private balcony or terrace", "balcony", true, "Balcony" },
                    { 16, "View", "Room with ocean/sea view", "waves", true, "Sea View" },
                    { 17, "View", "Room with city skyline view", "location_city", true, "City View" },
                    { 18, "Bedding", "Large king size bed", "king_bed", true, "King Size Bed" },
                    { 19, "Comfort", "Light-blocking window curtains", "curtains", true, "Blackout Curtains" },
                    { 20, "Convenience", "Iron and ironing board", "iron", true, "Iron & Board" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MyRoomAmenities_AmenityId",
                table: "MyRoomAmenities",
                column: "AmenityId");

            migrationBuilder.CreateIndex(
                name: "IX_MyRoomAmenities_RoomId_AmenityId",
                table: "MyRoomAmenities",
                columns: new[] { "RoomId", "AmenityId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MyRoomAmenities_MyRoomAmenitiesMaster_AmenityId",
                table: "MyRoomAmenities",
                column: "AmenityId",
                principalTable: "MyRoomAmenitiesMaster",
                principalColumn: "AmenityId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MyRoomAmenities_MyRoomAmenitiesMaster_AmenityId",
                table: "MyRoomAmenities");

            migrationBuilder.DropTable(
                name: "MyRoomAmenitiesMaster");

            migrationBuilder.DropIndex(
                name: "IX_MyRoomAmenities_AmenityId",
                table: "MyRoomAmenities");

            migrationBuilder.DropIndex(
                name: "IX_MyRoomAmenities_RoomId_AmenityId",
                table: "MyRoomAmenities");

            migrationBuilder.AddColumn<string>(
                name: "AmenityName",
                table: "MyRoomAmenities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MyRoomAmenities_RoomId",
                table: "MyRoomAmenities",
                column: "RoomId");
        }
    }
}
