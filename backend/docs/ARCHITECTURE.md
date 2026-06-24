# System Architecture - Smart Hotel Booking System

A detailed technical overview of the microservices architecture, database design, and inter-service communication patterns.

---

## Table of Contents

1. [High-Level Architecture](#high-level-architecture)
2. [Microservices Overview](#microservices-overview)
3. [Database Architecture](#database-architecture)
4. [Inter-Service Communication](#inter-service-communication)
5. [API Gateway Pattern](#api-gateway-pattern)
6. [Service Layer Pattern](#service-layer-pattern)
7. [Data Flow Examples](#data-flow-examples)
8. [Error Handling Strategy](#error-handling-strategy)
9. [Logging Architecture](#logging-architecture)
10. [Deployment Considerations](#deployment-considerations)

---

## High-Level Architecture

```
						   EXTERNAL CLIENTS
	+----------------------------------------------------------+
	|   Web Browser    |    Mobile App    |    Third Party     |
	+----------------------------------------------------------+
								|
								| HTTPS
								v
	+----------------------------------------------------------+
	|                      API GATEWAY                          |
	|                      (Ocelot)                             |
	|                      Port 5000                            |
	|                                                           |
	|   +---------------------------------------------------+   |
	|   |  - SSL Termination (production)                   |   |
	|   |  - JWT Token Validation                           |   |
	|   |  - Request Routing                                |   |
	|   |  - Rate Limiting (configurable)                   |   |
	|   |  - CORS Handling                                  |   |
	|   +---------------------------------------------------+   |
	+----------------------------------------------------------+
								|
			+-------------------+-------------------+
			|                   |                   |
			v                   v                   v
	+--------------+    +--------------+    +--------------+
	| AuthService  |    | HotelService |    | RoomService  |
	| Port 5001    |    | Port 5002    |    | Port 5003    |
	+--------------+    +--------------+    +--------------+
			|                   |                   |
			v                   v                   v
	+--------------+    +--------------+    +--------------+
	| SmartHotel_  |    | SmartHotel_  |    | SmartHotel_  |
	| AuthDb       |    | HotelDb      |    | RoomDb       |
	+--------------+    +--------------+    +--------------+

			+-------------------+-------------------+
			|                   |                   |
			v                   v                   v
	+--------------+    +--------------+    +--------------+
	|BookingService|    |PaymentService|    |ReviewService |
	| Port 5004    |    | Port 5005    |    | Port 5006    |
	+--------------+    +--------------+    +--------------+
			|                   |                   |
			v                   v                   v
	+--------------+    +--------------+    +--------------+
	| SmartHotel_  |    | SmartHotel_  |    | SmartHotel_  |
	| BookingDb    |    | PaymentDb    |    | ReviewDb     |
	+--------------+    +--------------+    +--------------+

						+--------------+
						|LoyaltyService|
						| Port 5007    |
						+--------------+
								|
								v
						+--------------+
						| SmartHotel_  |
						| LoyaltyDb    |
						+--------------+
```

---

## Microservices Overview

### Service Responsibilities

```
+------------------------------------------------------------------+
|  Service          |  Primary Responsibilities                     |
+------------------------------------------------------------------+
|                   |                                               |
|  AuthService      |  - User registration                          |
|  (Identity)       |  - Login / Authentication                     |
|                   |  - JWT token generation                       |
|                   |  - Password management                        |
|                   |  - User CRUD operations                       |
|                   |                                               |
+------------------------------------------------------------------+
|                   |                                               |
|  HotelService     |  - Hotel CRUD operations                      |
|  (Inventory)      |  - Amenities management                       |
|                   |  - Hotel search & filtering                   |
|                   |  - Rating aggregation                         |
|                   |                                               |
+------------------------------------------------------------------+
|                   |                                               |
|  RoomService      |  - Room CRUD operations                       |
|  (Inventory)      |  - Room availability tracking                 |
|                   |  - Room amenities                             |
|                   |  - Pricing management                         |
|                   |                                               |
+------------------------------------------------------------------+
|                   |                                               |
|  BookingService   |  - Reservation creation                       |
|  (Transactions)   |  - Date conflict detection                    |
|                   |  - Booking status management                  |
|                   |  - Cancellation handling                      |
|                   |                                               |
+------------------------------------------------------------------+
|                   |                                               |
|  PaymentService   |  - Payment processing                         |
|  (Financial)      |  - Transaction tracking                       |
|                   |  - Refund handling                            |
|                   |  - Payment status updates                     |
|                   |                                               |
+------------------------------------------------------------------+
|                   |                                               |
|  ReviewService    |  - Review submission                          |
|  (Feedback)       |  - Rating calculations                        |
|                   |  - Review moderation                          |
|                   |  - Hotel rating updates                       |
|                   |                                               |
+------------------------------------------------------------------+
|                   |                                               |
|  LoyaltyService   |  - Points earning                             |
|  (Rewards)        |  - Points redemption                          |
|                   |  - Tier management                            |
|                   |  - Transaction history                        |
|                   |                                               |
+------------------------------------------------------------------+
```

### Service Independence

Each service follows these principles:

```
+---------------------------+
|    SERVICE PRINCIPLES     |
+---------------------------+
|                           |
|  1. Own Database          |
|     - No shared tables    |
|     - Own migrations      |
|     - Data isolation      |
|                           |
|  2. Own Business Logic    |
|     - Self-contained      |
|     - Single responsibility|
|     - Domain-focused      |
|                           |
|  3. Independent Deploy    |
|     - Can deploy alone    |
|     - Version independent |
|     - Scale individually  |
|                           |
|  4. API Contracts         |
|     - Well-defined DTOs   |
|     - Versioned endpoints |
|     - Backward compatible |
|                           |
+---------------------------+
```

---

## Database Architecture

### Database Per Service Pattern

```
+-------------+     +-------------+     +-------------+
|  AuthDb     |     |  HotelDb    |     |  RoomDb     |
+-------------+     +-------------+     +-------------+
|             |     |             |     |             |
| MyUsers     |     | MyHotels    |     | MyRooms     |
|             |     | MyAmenities |     | MyRoom      |
|             |     | MyHotel     |     |  Amenities  |
|             |     |  Amenities  |     |             |
+-------------+     +-------------+     +-------------+

+-------------+     +-------------+     +-------------+
| BookingDb   |     | PaymentDb   |     | ReviewDb    |
+-------------+     +-------------+     +-------------+
|             |     |             |     |             |
| MyBookings  |     | MyPayments  |     | MyReviews   |
|             |     |             |     |             |
+-------------+     +-------------+     +-------------+

+-------------+
| LoyaltyDb   |
+-------------+
|             |
| MyLoyalty   |
|  Accounts   |
| MyPoint     |
|  Transactions|
| MyRedemptions|
+-------------+
```

### Entity Relationship Diagrams

#### AuthService Database

```
+------------------+
|     MyUsers      |
+------------------+
| PK UserId        |
|    Name          |
|    Email (unique)|
|    PasswordHash  |
|    Role          |
|    ContactNumber |
|    IsActive      |
|    CreatedAt     |
|    UpdatedAt     |
+------------------+
```

#### HotelService Database

```
+------------------+       +------------------+       +------------------+
|    MyHotels      |       | MyHotelAmenities |       |   MyAmenities    |
+------------------+       +------------------+       +------------------+
| PK HotelId       |<------| FK HotelId       |       | PK AmenityId     |
|    Name          |       | FK AmenityId     |------>|    Name          |
|    Location      |       +------------------+       |    Description   |
|    Address       |                                  |    Icon          |
|    City          |                                  |    Category      |
|    State         |                                  +------------------+
|    Country       |
|    ZipCode       |
|    ManagerId     |
|    Description   |
|    ImageUrl      |
|    ImageUrls     |
|    Rating        |
|    TotalReviews  |
|    ContactNumber |
|    Email         |
|    IsActive      |
|    CreatedAt     |
|    UpdatedAt     |
+------------------+
```

#### RoomService Database

```
+------------------+       +------------------+
|     MyRooms      |       | MyRoomAmenities  |
+------------------+       +------------------+
| PK RoomId        |<------| FK RoomId        |
|    HotelId       |       |    AmenityId     |
|    RoomNumber    |       |    AmenityName   |
|    RoomType      |       +------------------+
|    Description   |
|    PricePerNight |
|    MaxOccupancy  |
|    BedCount      |
|    BedType       |
|    FloorNumber   |
|    RoomSize      |
|    ImageUrl      |
|    ImageUrls     |
|    IsAvailable   |
|    IsActive      |
|    CreatedAt     |
|    UpdatedAt     |
+------------------+
```

#### BookingService Database

```
+------------------+
|   MyBookings     |
+------------------+
| PK BookingId     |
|    UserId        |
|    RoomId        |
|    HotelId       |
|    CheckInDate   |
|    CheckOutDate  |
|    NumberOfGuests|
|    TotalAmount   |
|    Status        |
|    SpecialRequests|
|    GuestName     |
|    GuestEmail    |
|    GuestPhone    |
|    PaymentId     |
|    CreatedAt     |
|    UpdatedAt     |
|    CancelledAt   |
|    CancelReason  |
+------------------+

Status Values:
  - Pending
  - Confirmed
  - CheckedIn
  - CheckedOut
  - Cancelled
```

#### PaymentService Database

```
+------------------+
|   MyPayments     |
+------------------+
| PK PaymentId     |
|    UserId        |
|    BookingId     |
|    Amount        |
|    Status        |
|    PaymentMethod |
|    TransactionId |
|    Currency      |
|    Description   |
|    CardLastFour  |
|    CardHolderName|
|    PaymentDate   |
|    RefundAmount  |
|    RefundReason  |
|    RefundedAt    |
|    CreatedAt     |
|    UpdatedAt     |
+------------------+

Status Values:
  - Pending
  - Completed
  - Failed
  - Refunded
```

---

## Inter-Service Communication

### Communication Patterns

```
+------------------------------------------------------------------+
|                  SYNCHRONOUS (HTTP/REST)                          |
+------------------------------------------------------------------+
|                                                                   |
|  Used when: Immediate response needed                             |
|                                                                   |
|  Examples:                                                        |
|    - BookingService -> RoomService (check availability)           |
|    - PaymentService -> BookingService (update booking status)     |
|    - ReviewService -> HotelService (update hotel rating)          |
|                                                                   |
+------------------------------------------------------------------+

	BookingService                         RoomService
		 |                                      |
		 |  GET /api/rooms/{id}/availability    |
		 |------------------------------------->|
		 |                                      |
		 |<-------------------------------------|
		 |  { available: true, price: 150 }     |
		 |                                      |
```

### HttpClient Configuration

Each service that needs to call others registers HttpClient:

```
Program.cs:
+------------------------------------------+
|  builder.Services.AddHttpClient          |
|    <IBookingManagementService,           |
|     BookingManagementService>();         |
+------------------------------------------+

appsettings.json:
+------------------------------------------+
|  "ServiceUrls": {                        |
|    "RoomService": "http://localhost:5003"|
|    "HotelService": "http://localhost:5002"
|  }                                       |
+------------------------------------------+
```

### Service Communication Map

```
+------------------------------------------------------------------+
|                                                                   |
|                         AuthService                               |
|                        (standalone)                               |
|                             |                                     |
|                             | issues JWT                          |
|                             v                                     |
|    +--------------------+   +--------------------+                |
|    |                    |   |                    |                |
|    |    HotelService <--+---+-> RoomService      |                |
|    |                    |       |                |                |
|    +--------+-----------+       |                |                |
|             ^                   |                |                |
|             |                   v                |                |
|    +--------+-----------+   +---+----------------+                |
|    |                    |   |                    |                |
|    |   ReviewService ------>|  BookingService    |                |
|    |   (updates rating) |   |  (checks rooms)    |                |
|    +--------------------+   +--------+-----------+                |
|                                      |                            |
|                                      v                            |
|                             +--------+-----------+                |
|                             |                    |                |
|                             |  PaymentService    |                |
|                             |  (updates booking) |                |
|                             +--------------------+                |
|                                                                   |
|                             +--------------------+                |
|                             |                    |                |
|                             |  LoyaltyService    |                |
|                             |  (standalone)      |                |
|                             +--------------------+                |
|                                                                   |
+------------------------------------------------------------------+
```

---

## API Gateway Pattern

### Ocelot Configuration

```
ocelot.json Structure:
+------------------------------------------------------------------+
|                                                                   |
|  {                                                                |
|    "Routes": [                                                    |
|      {                                                            |
|        "UpstreamPathTemplate": "/api/hotels/{everything}",        |
|        "DownstreamPathTemplate": "/api/hotels/{everything}",      |
|        "DownstreamHostAndPorts": [                                |
|          { "Host": "localhost", "Port": 5002 }                    |
|        ],                                                         |
|        "AuthenticationOptions": {                                 |
|          "AuthenticationProviderKey": "Bearer"                    |
|        }                                                          |
|      }                                                            |
|    ]                                                              |
|  }                                                                |
|                                                                   |
+------------------------------------------------------------------+
```

### Request Flow Through Gateway

```
	Client Request                    Gateway Processing
		 |                                   |
		 v                                   v
+------------------+              +--------------------+
| GET /api/hotels  |              | 1. Receive Request |
| Authorization:   |------------->| 2. Match Route     |
| Bearer eyJ...    |              | 3. Validate JWT    |
+------------------+              | 4. Forward Request |
								  +--------------------+
										   |
										   v
								  +--------------------+
								  | HotelService:5002  |
								  | GET /api/hotels    |
								  +--------------------+
										   |
										   v
								  +--------------------+
								  | Response to Client |
								  +--------------------+
```

---

## Service Layer Pattern

### Controller-Service Separation

```
+------------------------------------------------------------------+
|                        CONTROLLER                                 |
|  (Thin - handles HTTP concerns only)                              |
+------------------------------------------------------------------+
|                                                                   |
|  [HttpPost]                                                       |
|  public async Task<IActionResult> CreateHotel(CreateHotelDto dto) |
|  {                                                                |
|      var userId = GetUserIdFromToken();                           |
|      var result = await _hotelService.CreateHotelAsync(dto, userId);
|      return result.Success ? Ok(result) : BadRequest(result);     |
|  }                                                                |
|                                                                   |
+------------------------------------------------------------------+
								|
								v
+------------------------------------------------------------------+
|                         SERVICE                                   |
|  (Contains all business logic)                                    |
+------------------------------------------------------------------+
|                                                                   |
|  public async Task<ApiResponse<HotelResponseDto>>                 |
|      CreateHotelAsync(CreateHotelDto request, int managerId)      |
|  {                                                                |
|      // Validate duplicate                                        |
|      // Create entity                                             |
|      // Save to database                                          |
|      // Add amenities                                             |
|      // Return response                                           |
|  }                                                                |
|                                                                   |
+------------------------------------------------------------------+
```

### Project Structure Per Service

```
HotelService/
|
+-- Controllers/
|   +-- HotelsController.cs       # HTTP endpoints
|   +-- AmenitiesController.cs
|
+-- Services/
|   +-- IHotelManagementService.cs    # Interface
|   +-- HotelManagementService.cs     # Implementation
|   +-- IAmenityService.cs
|   +-- AmenityService.cs
|
+-- Models/
|   +-- Hotel.cs                  # EF Entity
|   +-- Amenity.cs
|   +-- HotelAmenity.cs
|
+-- DTOs/
|   +-- CreateHotelDto.cs         # Input DTOs
|   +-- UpdateHotelDto.cs
|   +-- HotelResponseDto.cs       # Output DTOs
|   +-- HotelSearchDto.cs
|   +-- ApiResponse.cs            # Standard response
|
+-- Data/
|   +-- HotelDbContext.cs         # EF DbContext
|
+-- Properties/
|   +-- launchSettings.json
|
+-- Program.cs                    # Startup
+-- appsettings.json              # Configuration
```

---

## Data Flow Examples

### Complete Booking Flow

```
+------------------------------------------------------------------+
|  STEP 1: User searches for hotels                                 |
+------------------------------------------------------------------+

Client                    Gateway                  HotelService
   |                         |                          |
   |  GET /api/hotels        |                          |
   |  ?city=NYC              |                          |
   |------------------------>|                          |
   |                         |  GET /api/hotels?city=NYC|
   |                         |------------------------->|
   |                         |                          |
   |                         |<-------------------------|
   |                         |  [hotel1, hotel2, ...]   |
   |<------------------------|                          |
   |  Hotel list             |                          |


+------------------------------------------------------------------+
|  STEP 2: User selects hotel, views rooms                          |
+------------------------------------------------------------------+

Client                    Gateway                  RoomService
   |                         |                          |
   |  GET /api/rooms         |                          |
   |  ?hotelId=5             |                          |
   |------------------------>|                          |
   |                         |  GET /api/rooms?hotelId=5|
   |                         |------------------------->|
   |                         |                          |
   |                         |<-------------------------|
   |                         |  [room1, room2, ...]     |
   |<------------------------|                          |
   |  Room list              |                          |


+------------------------------------------------------------------+
|  STEP 3: User creates booking                                     |
+------------------------------------------------------------------+

Client                    Gateway              BookingService    RoomService
   |                         |                       |               |
   |  POST /api/bookings     |                       |               |
   |  { roomId: 10,          |                       |               |
   |    checkIn: "2024-06-01"|                       |               |
   |    checkOut: "2024-06-05" }                     |               |
   |------------------------>|                       |               |
   |                         |  POST /api/bookings   |               |
   |                         |---------------------->|               |
   |                         |                       |               |
   |                         |                       | Check room    |
   |                         |                       | availability  |
   |                         |                       |-------------->|
   |                         |                       |               |
   |                         |                       |<--------------|
   |                         |                       | { available } |
   |                         |                       |               |
   |                         |                       | Create booking|
   |                         |                       | in database   |
   |                         |                       |               |
   |                         |<----------------------|               |
   |                         |  { bookingId: 42 }    |               |
   |<------------------------|                       |               |
   |  Booking confirmed      |                       |               |


+------------------------------------------------------------------+
|  STEP 4: User makes payment                                       |
+------------------------------------------------------------------+

Client                    Gateway              PaymentService   BookingService
   |                         |                       |               |
   |  POST /api/payments     |                       |               |
   |  { bookingId: 42,       |                       |               |
   |    amount: 600,         |                       |               |
   |    cardNumber: "4111..."|                       |               |
   |------------------------>|                       |               |
   |                         |  POST /api/payments   |               |
   |                         |---------------------->|               |
   |                         |                       |               |
   |                         |                       | Process payment|
   |                         |                       |               |
   |                         |                       | Update booking |
   |                         |                       | status        |
   |                         |                       |-------------->|
   |                         |                       |               |
   |                         |                       |<--------------|
   |                         |                       | { confirmed } |
   |                         |                       |               |
   |                         |<----------------------|               |
   |                         |  { paymentId: 99 }    |               |
   |<------------------------|                       |               |
   |  Payment successful     |                       |               |
```

---

## Error Handling Strategy

### Standard API Response

```
+------------------------------------------------------------------+
|  SUCCESS RESPONSE                                                 |
+------------------------------------------------------------------+
|  {                                                                |
|    "success": true,                                               |
|    "message": "Hotel created successfully",                       |
|    "data": {                                                      |
|      "hotelId": 1,                                                |
|      "name": "Grand Hotel",                                       |
|      ...                                                          |
|    }                                                              |
|  }                                                                |
+------------------------------------------------------------------+

+------------------------------------------------------------------+
|  ERROR RESPONSE                                                   |
+------------------------------------------------------------------+
|  {                                                                |
|    "success": false,                                              |
|    "message": "Hotel with this name already exists at location",  |
|    "data": null                                                   |
|  }                                                                |
+------------------------------------------------------------------+
```

### Error Handling Flow

```
Controller                    Service                     Database
	|                            |                            |
	|  Call service method       |                            |
	|--------------------------->|                            |
	|                            |  Execute business logic    |
	|                            |--------------------------->|
	|                            |                            |
	|                            |  Exception occurs          |
	|                            |<---------------------------|
	|                            |                            |
	|                            |  Catch exception           |
	|                            |  Return ApiResponse.Fail   |
	|<---------------------------|                            |
	|                            |                            |
	|  Return BadRequest(result) |                            |
	|                            |                            |
```

---

## Logging Architecture

### Serilog Configuration

```
+------------------------------------------------------------------+
|  Log Output: Console + File                                       |
+------------------------------------------------------------------+

Console Output:
  [14:32:15 INF] HotelService | Starting Hotel Service...
  [14:32:16 INF] HotelService | Hotel Service started on port 5002

File Output (Logs/hotelservice-20240530.log):
  2024-05-30 14:32:15.123 +00:00 [INF] HotelService | Starting...
  2024-05-30 14:32:16.456 +00:00 [INF] HotelService | Started...
```

### Log Levels by Environment

```
+------------------+------------------+------------------+
|     Level        |   Development    |   Production     |
+------------------+------------------+------------------+
|  Debug           |       Yes        |       No         |
|  Information     |       Yes        |       Yes        |
|  Warning         |       Yes        |       Yes        |
|  Error           |       Yes        |       Yes        |
|  Fatal           |       Yes        |       Yes        |
+------------------+------------------+------------------+
```

---

## Deployment Considerations

### Local Development

```
+------------------------------------------------------------------+
|  All services on localhost with different ports                   |
+------------------------------------------------------------------+

  localhost:5000  --> API Gateway
  localhost:5001  --> AuthService
  localhost:5002  --> HotelService
  localhost:5003  --> RoomService
  localhost:5004  --> BookingService
  localhost:5005  --> PaymentService
  localhost:5006  --> ReviewService
  localhost:5007  --> LoyaltyService
```

### Docker Deployment (Future)

```
+------------------------------------------------------------------+
|  Each service as a container                                      |
+------------------------------------------------------------------+

  docker-compose.yml:

  services:
	gateway:
	  build: ./ApiGateway
	  ports: ["5000:80"]

	auth:
	  build: ./AuthService
	  ports: ["5001:80"]

	hotel:
	  build: ./HotelService
	  ports: ["5002:80"]

	# ... more services

	sqlserver:
	  image: mcr.microsoft.com/mssql/server:2022-latest
	  ports: ["1433:1433"]
```

### Scaling Considerations

```
+------------------------------------------------------------------+
|  HORIZONTAL SCALING                                               |
+------------------------------------------------------------------+
|                                                                   |
|  High traffic services can have multiple instances:               |
|                                                                   |
|                    Load Balancer                                  |
|                         |                                         |
|           +-------------+-------------+                           |
|           |             |             |                           |
|           v             v             v                           |
|    +----------+   +----------+   +----------+                     |
|    | Hotel    |   | Hotel    |   | Hotel    |                     |
|    | Service  |   | Service  |   | Service  |                     |
|    | (inst 1) |   | (inst 2) |   | (inst 3) |                     |
|    +----------+   +----------+   +----------+                     |
|                                                                   |
+------------------------------------------------------------------+
```

---

## Technology Decisions

### Why These Choices?

```
+------------------+--------------------------------------------------+
|  Technology      |  Reason                                          |
+------------------+--------------------------------------------------+
|  .NET 8.0        |  Latest LTS, performance, cross-platform         |
|  Entity Framework|  Code-first, migrations, LINQ support            |
|  SQL Server      |  Enterprise-ready, reliable, familiar            |
|  Ocelot          |  Simple, .NET native, JWT support                |
|  Serilog         |  Structured logging, multiple sinks              |
|  BCrypt          |  Secure password hashing, industry standard      |
|  Swagger         |  API documentation, testing UI                   |
+------------------+--------------------------------------------------+
```

---

*Document Version: 1.0*
*Last Updated: May 2024*
