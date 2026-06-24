# Smart Hotel Booking System

A production-ready microservices-based hotel booking platform built with .NET 8.0, featuring JWT authentication, Entity Framework Core, and comprehensive API documentation.

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Services](#services)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [Database Setup](#database-setup)
- [Running the Application](#running-the-application)
- [API Documentation](#api-documentation)
- [Authentication](#authentication)
- [Project Structure](#project-structure)
- [Configuration](#configuration)
- [Logging](#logging)
- [Contributing](#contributing)

---

## Overview

Smart Hotel Booking System is a complete hotel management solution that handles everything from user registration to booking, payments, reviews, and loyalty rewards. The system follows microservices architecture principles, ensuring scalability, maintainability, and independent deployment of each service.

### Key Features

- **User Management**: Registration, login, role-based access (Admin, Manager, Guest)
- **Hotel Management**: CRUD operations for hotels with amenities
- **Room Management**: Room inventory with availability tracking
- **Booking System**: Date-based reservations with conflict detection
- **Payment Processing**: Secure payment handling with transaction tracking
- **Reviews & Ratings**: Customer feedback with rating aggregation
- **Loyalty Program**: Points earning and redemption system

---

## Architecture

```
									+------------------+
									|   Client Apps    |
									| (Web/Mobile/API) |
									+--------+---------+
											 |
											 v
									+------------------+
									|   API Gateway    |
									|   (Port 5000)    |
									|   JWT Validation |
									+--------+---------+
											 |
			  +------------------------------+------------------------------+
			  |              |              |              |               |
			  v              v              v              v               v
	 +----------------+ +----------+ +----------+ +----------+ +------------+
	 | Auth Service   | | Hotel    | | Room     | | Booking  | | Payment    |
	 | (Port 5001)    | | Service  | | Service  | | Service  | | Service    |
	 | - Registration | | (5002)   | | (5003)   | | (5004)   | | (5005)     |
	 | - Login        | | - CRUD   | | - CRUD   | | - CRUD   | | - Process  |
	 | - JWT Issue    | | - Search | | - Avail. | | - Dates  | | - Refund   |
	 +-------+--------+ +----+-----+ +----+-----+ +----+-----+ +-----+------+
			 |               |            |            |             |
			 v               v            v            v             v
	 +----------------+ +----------+ +----------+ +----------+ +------------+
	 | SmartHotel_    | | SmartH_  | | SmartH_  | | SmartH_  | | SmartH_    |
	 | AuthDb         | | HotelDb  | | RoomDb   | | BookingDb| | PaymentDb  |
	 +----------------+ +----------+ +----------+ +----------+ +------------+

			  +------------------------------+
			  |              |               |
			  v              v               v
	 +----------------+ +----------+ +------------+
	 | Review Service | | Loyalty  |              |
	 | (Port 5006)    | | Service  |              |
	 | - Ratings      | | (5007)   |              |
	 | - Comments     | | - Points |              |
	 +-------+--------+ +----+-----+              |
			 |               |                    |
			 v               v                    |
	 +----------------+ +----------+              |
	 | SmartHotel_    | | SmartH_  |              |
	 | ReviewDb       | | LoyaltyDb|              |
	 +----------------+ +----------+              |
```

---

## Services

| Service | Port | Database | Description |
|---------|------|----------|-------------|
| **API Gateway** | 5000 | - | Routes requests, validates JWT tokens |
| **Auth Service** | 5001 | SmartHotel_AuthDb | User authentication & authorization |
| **Hotel Service** | 5002 | SmartHotel_HotelDb | Hotel & amenities management |
| **Room Service** | 5003 | SmartHotel_RoomDb | Room inventory & availability |
| **Booking Service** | 5004 | SmartHotel_BookingDb | Reservation management |
| **Payment Service** | 5005 | SmartHotel_PaymentDb | Payment processing |
| **Review Service** | 5006 | SmartHotel_ReviewDb | Reviews & ratings |
| **Loyalty Service** | 5007 | SmartHotel_LoyaltyDb | Loyalty points & rewards |

---

## Tech Stack

- **.NET 8.0** - Core framework
- **Entity Framework Core 8.0** - ORM with Code-First migrations
- **SQL Server** - Database
- **Ocelot 23.2.2** - API Gateway
- **JWT Bearer Authentication** - Security
- **BCrypt.Net** - Password hashing
- **Serilog** - Structured logging
- **Swagger/OpenAPI** - API documentation

---

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express or higher)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or VS Code
- Git

### Clone the Repository

```bash
git clone https://github.com/yourusername/SmartHotelBooking.git
cd SmartHotelBooking
```

---

## Database Setup

### 1. Update Connection Strings

Each service has its own `appsettings.json`. Update the connection string for your SQL Server:

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=localhost;Database=SmartHotel_AuthDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### 2. Run Migrations

Apply migrations for each service:

```bash
# Auth Service
cd AuthService
dotnet ef database update

# Hotel Service
cd ../HotelService
dotnet ef database update

# Room Service
cd ../RoomService
dotnet ef database update

# Booking Service
cd ../BookingService
dotnet ef database update

# Payment Service
cd ../PaymentService
dotnet ef database update

# Review Service
cd ../ReviewService
dotnet ef database update

# Loyalty Service
cd ../LoyaltyService
dotnet ef database update
```

---

## Running the Application

### Option 1: Run All Services (Recommended)

Use multiple terminals or configure Visual Studio to run multiple startup projects:

```bash
# Terminal 1 - API Gateway
cd ApiGateway && dotnet run

# Terminal 2 - Auth Service
cd AuthService && dotnet run

# Terminal 3 - Hotel Service
cd HotelService && dotnet run

# ... repeat for other services
```

### Option 2: Visual Studio Multiple Startup

1. Right-click on Solution
2. Select "Set Startup Projects..."
3. Choose "Multiple startup projects"
4. Set Action to "Start" for all projects
5. Press F5

### Service URLs

| Service | URL |
|---------|-----|
| API Gateway | http://localhost:5000 |
| Auth Service | http://localhost:5001/swagger |
| Hotel Service | http://localhost:5002/swagger |
| Room Service | http://localhost:5003/swagger |
| Booking Service | http://localhost:5004/swagger |
| Payment Service | http://localhost:5005/swagger |
| Review Service | http://localhost:5006/swagger |
| Loyalty Service | http://localhost:5007/swagger |

---

## API Documentation

Each service exposes Swagger UI for interactive API documentation:

- **Auth Service**: `http://localhost:5001/swagger`
- **Hotel Service**: `http://localhost:5002/swagger`
- And so on...

### Sample API Endpoints

**Auth Service**
```
POST /api/auth/register    - Register new user
POST /api/auth/login       - Login and get JWT token
GET  /api/auth/users       - List all users (Admin only)
```

**Hotel Service**
```
GET    /api/hotels         - List all hotels
POST   /api/hotels         - Create hotel (Admin/Manager)
GET    /api/hotels/{id}    - Get hotel by ID
PUT    /api/hotels/{id}    - Update hotel
DELETE /api/hotels/{id}    - Delete hotel
```

**Booking Service**
```
POST /api/bookings         - Create booking
GET  /api/bookings/user    - Get user's bookings
PUT  /api/bookings/{id}/cancel - Cancel booking
```

---

## Authentication

See [docs/JWT_AUTHENTICATION.md](docs/JWT_AUTHENTICATION.md) for detailed JWT flow documentation.

### Quick Start

1. **Register a User**
```bash
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"name":"John Doe","email":"john@example.com","password":"Pass123!","role":"Guest"}'
```

2. **Login to Get Token**
```bash
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"john@example.com","password":"Pass123!"}'
```

3. **Use Token in Requests**
```bash
curl -X GET http://localhost:5000/api/hotels \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

---

## Project Structure

```
SmartHotelBooking/
├── ApiGateway/
│   ├── Settings/
│   ├── ocelot.json
│   └── Program.cs
├── AuthService/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Models/
│   ├── Services/
│   └── Settings/
├── HotelService/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Models/
│   └── Services/
├── RoomService/
├── BookingService/
├── PaymentService/
├── ReviewService/
├── LoyaltyService/
├── SharedLibrary/
│   ├── Constants/
│   └── DTOs/
├── docs/
│   ├── JWT_AUTHENTICATION.md
│   └── ARCHITECTURE.md
└── README.md
```

---

## Configuration

### JWT Settings (appsettings.json)

```json
{
  "JwtSettings": {
	"SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
	"Issuer": "SmartHotelBooking",
	"Audience": "SmartHotelUsers",
	"ExpirationInMinutes": 60
  }
}
```

> **Important**: Use the same `SecretKey`, `Issuer`, and `Audience` across all services.

### Ocelot Gateway Routes (ocelot.json)

Routes are configured in `ApiGateway/ocelot.json`. Each route maps external URLs to internal service endpoints.

---

## Logging

The system uses **Serilog** for structured logging with both console and file outputs.

### Log Files

Each service writes logs to its own file:
- `Logs/apigateway-{date}.log`
- `Logs/authservice-{date}.log`
- `Logs/hotelservice-{date}.log`
- etc.

### Log Format

```
[14:32:15 INF] AuthService | User john@example.com logged in successfully
[14:32:16 WRN] BookingService | Room 101 unavailable for requested dates
[14:32:17 ERR] PaymentService | Payment failed for booking #42
```

### Log Levels

- **Debug**: Detailed diagnostic information
- **Information**: General operational events
- **Warning**: Abnormal or unexpected events
- **Error**: Error events that allow the app to continue
- **Fatal**: Critical errors causing app termination

---

## Duplicate Protection

The system implements comprehensive duplicate protection:

| Service | Protection |
|---------|------------|
| Hotel | Same Name + Location blocked |
| Room | Same RoomNumber in Hotel blocked |
| Booking | Overlapping dates blocked |
| Payment | Duplicate payment for booking blocked |
| Review | One review per user per hotel |
| Loyalty | One account per user, one redemption per booking |

---

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## Contact

For questions or support, please open an issue on GitHub.

---

*Built with passion for clean architecture and microservices design patterns.*
# backednwee
