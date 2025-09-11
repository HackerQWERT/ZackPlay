using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialFlightBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "airports",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, comment: "机场三字码，如PEK、SHA"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "机场名称"),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "所在城市"),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "所在国家"),
                    TimeZone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "时区"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, comment: "是否启用"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_airports", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "flights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FlightNumber = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false, comment: "航班号，如CZ3456"),
                    AirlineCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, comment: "航空公司代码，如CZ"),
                    AirlineName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "航空公司名称"),
                    DepartureAirportCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, comment: "出发机场代码"),
                    DepartureTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DepartureTerminal = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, comment: "出发航站楼"),
                    ArrivalAirportCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, comment: "到达机场代码"),
                    ArrivalTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArrivalTerminal = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, comment: "到达航站楼"),
                    AircraftType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "机型，如A320"),
                    TotalSeats = table.Column<int>(type: "int", nullable: false),
                    AvailableSeats = table.Column<int>(type: "int", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false, comment: "基础价格"),
                    Status = table.Column<int>(type: "int", nullable: false, comment: "航班状态：0=计划中,1=登机中,2=已起飞,3=飞行中,4=已到达,5=延误,6=取消"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flights", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "passengers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "名字"),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "姓氏"),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false, comment: "性别：0=男,1=女,2=其他"),
                    PassportNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "护照号码"),
                    PassportCountry = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, comment: "护照签发国代码"),
                    PassportExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Nationality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "国籍"),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "邮箱地址"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "电话号码"),
                    Type = table.Column<int>(type: "int", nullable: false, comment: "乘客类型：0=婴儿,1=儿童,2=成人,3=老人"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_passengers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "flight_bookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingReference = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "预订参考号"),
                    FlightId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PassengerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeatsCount = table.Column<int>(type: "int", nullable: false),
                    CabinClass = table.Column<int>(type: "int", nullable: false, comment: "舱位等级：0=经济舱,1=高端经济舱,2=商务舱,3=头等舱"),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false, comment: "单价"),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, comment: "总金额"),
                    SpecialRequests = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, comment: "特殊要求"),
                    Status = table.Column<int>(type: "int", nullable: false, comment: "预订状态：0=待确认,1=已确认,2=已登机,3=已取消"),
                    BookingTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfirmationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckInTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false, comment: "支付状态：0=待支付,1=已支付,2=支付失败,3=已退款"),
                    PaymentTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "支付参考号"),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "取消原因"),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, comment: "退款金额")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_flight_bookings_flights_FlightId",
                        column: x => x.FlightId,
                        principalTable: "flights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_flight_bookings_passengers_PassengerId",
                        column: x => x.PassengerId,
                        principalTable: "passengers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_airports_city",
                table: "airports",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_airports_country",
                table: "airports",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_flight",
                table: "flight_bookings",
                column: "FlightId");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_passenger",
                table: "flight_bookings",
                column: "PassengerId");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_payment_status",
                table: "flight_bookings",
                column: "PaymentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_reference_unique",
                table: "flight_bookings",
                column: "BookingReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bookings_status",
                table: "flight_bookings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_time",
                table: "flight_bookings",
                column: "BookingTime");

            migrationBuilder.CreateIndex(
                name: "IX_flights_airline",
                table: "flights",
                column: "AirlineCode");

            migrationBuilder.CreateIndex(
                name: "IX_flights_number_departure",
                table: "flights",
                columns: new[] { "FlightNumber", "DepartureTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_flights_route_time",
                table: "flights",
                columns: new[] { "DepartureAirportCode", "ArrivalAirportCode", "DepartureTime" });

            migrationBuilder.CreateIndex(
                name: "IX_flights_status",
                table: "flights",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_passengers_email_unique",
                table: "passengers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_passengers_passport_unique",
                table: "passengers",
                column: "PassportNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "airports");

            migrationBuilder.DropTable(
                name: "flight_bookings");

            migrationBuilder.DropTable(
                name: "flights");

            migrationBuilder.DropTable(
                name: "passengers");
        }
    }
}
