# JWT Authentication Flow - Smart Hotel Booking System

This document explains how JSON Web Token (JWT) authentication works across all microservices in the Smart Hotel Booking System.

---

## Table of Contents

1. [What is JWT?](#what-is-jwt)
2. [Why JWT for Microservices?](#why-jwt-for-microservices)
3. [Token Structure](#token-structure)
4. [Authentication Flow](#authentication-flow)
5. [Service-by-Service Implementation](#service-by-service-implementation)
6. [Gateway Token Validation](#gateway-token-validation)
7. [Role-Based Authorization](#role-based-authorization)
8. [Token Lifecycle](#token-lifecycle)
9. [Security Considerations](#security-considerations)
10. [Troubleshooting](#troubleshooting)

---

## What is JWT?

JWT (JSON Web Token) is an open standard (RFC 7519) for securely transmitting information between parties as a JSON object. In our system, it serves as a "digital passport" that proves who you are and what you're allowed to do.

Think of it like a concert wristband - once you get it at the entrance (login), you can access different areas (services) without showing your ID again.

---

## Why JWT for Microservices?

In a microservices architecture, we chose JWT because:

1. **Stateless**: No session storage needed on servers
2. **Self-contained**: Token carries all user info
3. **Scalable**: Each service can validate independently
4. **Cross-service**: One token works across all services
5. **Decoupled**: Services don't need to call AuthService for every request

---

## Token Structure

A JWT consists of three parts separated by dots (.):

```
xxxxx.yyyyy.zzzzz
  |      |      |
  |      |      +-- Signature (verification)
  |      +--------- Payload (user data)
  +---------------- Header (algorithm info)
```

### Our Token Payload

When a user logs in, the token contains:

```json
{
  "nameid": "42",                           // User ID
  "email": "john@example.com",              // User email
  "name": "John Doe",                       // Display name
  "role": "Guest",                          // User role
  "exp": 1699999999,                        // Expiration time
  "iss": "SmartHotelBooking",               // Issuer
  "aud": "SmartHotelUsers"                  // Audience
}
```

---

## Authentication Flow

### Complete Flow Diagram

```
+--------+                                  +-------------+
|        |     1. Register (first time)     |             |
| Client | -------------------------------->| AuthService |
|        |     POST /api/auth/register      |  Port 5001  |
|        |                                  |             |
|        |<---------------------------------|             |
|        |     User created successfully    +------+------+
|        |                                         |
|        |     2. Login with credentials           |
|        | ----------------------------------------+
|        |     POST /api/auth/login               |
|        |                                        v
|        |                              +------------------+
|        |                              |  Validate creds  |
|        |                              |  Generate JWT    |
|        |                              +------------------+
|        |                                        |
|        |<---------------------------------------+
|        |     JWT Token returned
|        |
|        |     3. Access protected resource
|        | ----------------------------------------+
|        |     GET /api/hotels                    |
|        |     Authorization: Bearer <token>      |
|        |                                        v
|        |                              +------------------+
|        |                              |   API Gateway    |
|        |                              |   Port 5000      |
|        |                              +--------+---------+
|        |                                       |
|        |                              +--------v---------+
|        |                              | Validate JWT     |
|        |                              | - Check signature|
|        |                              | - Check expiry   |
|        |                              | - Check issuer   |
|        |                              +--------+---------+
|        |                                       |
|        |                              +--------v---------+
|        |                              |  Forward to      |
|        |                              |  HotelService    |
|        |                              |  Port 5002       |
|        |                              +--------+---------+
|        |                                       |
|        |<--------------------------------------+
|        |     Response data
+--------+
```

### Step-by-Step Breakdown

#### Step 1: User Registration

```
Client                          AuthService
   |                                 |
   |  POST /api/auth/register        |
   |  {                              |
   |    "name": "John Doe",          |
   |    "email": "john@example.com", |
   |    "password": "Pass123!",      |
   |    "role": "Guest"              |
   |  }                              |
   |-------------------------------->|
   |                                 |
   |                    +------------+-------------+
   |                    | 1. Check email exists   |
   |                    | 2. Hash password        |
   |                    | 3. Save to MyUsers      |
   |                    +------------+-------------+
   |                                 |
   |<--------------------------------|
   |  {                              |
   |    "success": true,             |
   |    "data": { userId: 42, ... }  |
   |  }                              |
```

#### Step 2: User Login

```
Client                          AuthService
   |                                 |
   |  POST /api/auth/login           |
   |  {                              |
   |    "email": "john@example.com", |
   |    "password": "Pass123!"       |
   |  }                              |
   |-------------------------------->|
   |                                 |
   |                    +------------+-------------+
   |                    | 1. Find user by email   |
   |                    | 2. Verify password hash |
   |                    | 3. Generate JWT token   |
   |                    +------------+-------------+
   |                                 |
   |<--------------------------------|
   |  {                              |
   |    "success": true,             |
   |    "data": {                    |
   |      "token": "eyJhbG...",      |
   |      "email": "john@example.com"|
   |      "role": "Guest",           |
   |      "expiresAt": "2024-..."    |
   |    }                            |
   |  }                              |
```

#### Step 3: Accessing Protected Resources

```
Client                   API Gateway              HotelService
   |                          |                        |
   |  GET /api/hotels         |                        |
   |  Authorization:          |                        |
   |  Bearer eyJhbG...        |                        |
   |------------------------->|                        |
   |                          |                        |
   |             +------------+------------+           |
   |             | Validate JWT:           |           |
   |             | - Signature valid?      |           |
   |             | - Not expired?          |           |
   |             | - Correct issuer?       |           |
   |             +------------+------------+           |
   |                          |                        |
   |                          |  Forward request       |
   |                          |  (with user claims)    |
   |                          |----------------------->|
   |                          |                        |
   |                          |<-----------------------|
   |                          |  Hotel list            |
   |<-------------------------|                        |
   |  Hotel data              |                        |
```

---

## Service-by-Service Implementation

### AuthService (Token Issuer)

Location: `AuthService/Services/UserService.cs`

The AuthService is the ONLY service that creates tokens:

```
+-------------------+
|   AuthService     |
|   Port 5001       |
+-------------------+
|                   |
| Responsibilities: |
| - User CRUD       |
| - Password hash   |
| - Token creation  |
|                   |
| Token Claims:     |
| - NameIdentifier  |
| - Email           |
| - Name            |
| - Role            |
|                   |
+-------------------+
```

**Token Generation Code Flow:**

```
Login Request
	 |
	 v
+--------------------+
| Find User by Email |
+--------------------+
	 |
	 v
+--------------------+
| Verify Password    |
| (BCrypt compare)   |
+--------------------+
	 |
	 v
+--------------------+
| Create Claims:     |
| - UserId           |
| - Email            |
| - Name             |
| - Role             |
+--------------------+
	 |
	 v
+--------------------+
| Create JWT:        |
| - Set Issuer       |
| - Set Audience     |
| - Set Expiration   |
| - Sign with Key    |
+--------------------+
	 |
	 v
+--------------------+
| Return Token       |
+--------------------+
```

### API Gateway (Token Validator)

Location: `ApiGateway/Program.cs`

The Gateway validates ALL incoming tokens:

```
+-------------------+
|   API Gateway     |
|   Port 5000       |
+-------------------+
|                   |
| For EVERY request:|
|                   |
| 1. Extract token  |
|    from header    |
|                   |
| 2. Validate:      |
|    - Signature    |
|    - Expiration   |
|    - Issuer       |
|    - Audience     |
|                   |
| 3. If valid:      |
|    Forward to     |
|    backend service|
|                   |
| 4. If invalid:    |
|    Return 401     |
|                   |
+-------------------+
```

### Backend Services (Token Consumers)

Each backend service reads claims from validated tokens:

```
+-------------------+     +-------------------+     +-------------------+
|   HotelService    |     |   BookingService  |     |  PaymentService   |
|   Port 5002       |     |   Port 5004       |     |   Port 5005       |
+-------------------+     +-------------------+     +-------------------+
|                   |     |                   |     |                   |
| Trust Gateway's   |     | Trust Gateway's   |     | Trust Gateway's   |
| validation        |     | validation        |     | validation        |
|                   |     |                   |     |                   |
| Extract from      |     | Extract from      |     | Extract from      |
| claims:           |     | claims:           |     | claims:           |
| - UserId          |     | - UserId          |     | - UserId          |
| - Role            |     | - Role            |     | - Role            |
|                   |     |                   |     |                   |
| Apply role-based  |     | Apply role-based  |     | Apply role-based  |
| authorization     |     | authorization     |     | authorization     |
+-------------------+     +-------------------+     +-------------------+
```

---

## Gateway Token Validation

### Ocelot Configuration

The API Gateway uses Ocelot for routing with JWT validation:

```
ocelot.json
+------------------------------------------+
|                                          |
|  Route: /api/hotels/* -> HotelService    |
|  Authentication: Bearer                  |
|                                          |
|  Route: /api/rooms/* -> RoomService      |
|  Authentication: Bearer                  |
|                                          |
|  Route: /api/auth/* -> AuthService       |
|  Authentication: None (public)           |
|                                          |
+------------------------------------------+
```

### Validation Flow at Gateway

```
Incoming Request
	   |
	   v
+------+-------+
| Has Auth     |
| Header?      |
+------+-------+
	   |
   +---+---+
   |       |
  No      Yes
   |       |
   v       v
+-------+ +------------+
| 401   | | Extract    |
| Error | | Token      |
+-------+ +-----+------+
				|
				v
		+-------+-------+
		| Validate      |
		| Signature     |
		+-------+-------+
				|
		  +-----+-----+
		  |           |
	   Invalid      Valid
		  |           |
		  v           v
	+--------+  +-----+------+
	| 401    |  | Check      |
	| Error  |  | Expiration |
	+--------+  +-----+------+
					  |
				+-----+-----+
				|           |
			 Expired     Valid
				|           |
				v           v
		  +--------+  +-----+------+
		  | 401    |  | Forward to |
		  | Error  |  | Service    |
		  +--------+  +------------+
```

---

## Role-Based Authorization

### User Roles

```
+----------------+
|     Admin      |  Full access to everything
+----------------+
		|
		v
+----------------+
|    Manager     |  Can manage hotels/rooms
+----------------+
		|
		v
+----------------+
|     Guest      |  Can book and review
+----------------+
```

### Authorization Matrix

```
+----------------------+-------+---------+-------+
| Action               | Admin | Manager | Guest |
+----------------------+-------+---------+-------+
| Create Hotel         |   X   |    X    |       |
| Update Hotel         |   X   |    X*   |       |
| Delete Hotel         |   X   |    X*   |       |
| Create Room          |   X   |    X    |       |
| View Hotels/Rooms    |   X   |    X    |   X   |
| Create Booking       |   X   |    X    |   X   |
| Cancel Own Booking   |   X   |    X    |   X   |
| Process Payment      |   X   |    X    |   X   |
| Create Review        |   X   |    X    |   X   |
| Manage All Users     |   X   |         |       |
+----------------------+-------+---------+-------+

* Manager can only modify their own hotels
```

### Controller Authorization

Example from HotelController:

```
[Authorize(Roles = "Admin,Manager")]
public class HotelsController
{
	[HttpPost]           // Only Admin/Manager
	public async Task<IActionResult> CreateHotel(...)

	[HttpGet]            // Anyone authenticated
	[AllowAnonymous]     // Or even anonymous
	public async Task<IActionResult> GetHotels(...)
}
```

---

## Token Lifecycle

```
	+-------------+
	|   Created   |  Login successful
	+------+------+
		   |
		   v
	+------+------+
	|   Active    |  Can be used for requests
	+------+------+
		   |
		   | (after ExpirationInMinutes)
		   v
	+------+------+
	|   Expired   |  Returns 401 Unauthorized
	+-------------+

	Note: There is no "refresh token" in current implementation.
	User must login again after expiration.
```

### Default Expiration

```json
{
  "JwtSettings": {
	"ExpirationInMinutes": 60
  }
}
```

---

## Security Considerations

### What We Implemented

1. **Strong Secret Key**: 256-bit minimum
2. **HMAC-SHA256 Signing**: Industry standard
3. **Issuer/Audience Validation**: Prevents token reuse
4. **Password Hashing**: BCrypt with salt
5. **HTTPS Recommended**: For production

### Configuration Security

```
IMPORTANT: In production, store secrets securely!

Development (appsettings.json):
  "SecretKey": "YourDevelopmentSecretKey..."

Production (Environment Variables):
  JWT_SECRET_KEY=<from-azure-keyvault>
```

### Secret Key Requirements

```
+------------------------------------------+
|  Secret Key Guidelines                   |
+------------------------------------------+
|  - Minimum 32 characters                 |
|  - Include letters, numbers, symbols     |
|  - Never commit to source control        |
|  - Rotate periodically                   |
|  - Same key across ALL services          |
+------------------------------------------+
```

---

## Troubleshooting

### Common Issues

#### 1. 401 Unauthorized

```
Problem: Token rejected
Check:
  [ ] Token not expired?
  [ ] Same SecretKey in all services?
  [ ] Same Issuer in all services?
  [ ] Same Audience in all services?
  [ ] Token included correctly in header?
	  Authorization: Bearer eyJhbG...
```

#### 2. Token Not Working After Service Restart

```
Problem: Different secret keys
Solution: Ensure appsettings.json has
		 identical JwtSettings in all services
```

#### 3. "Bearer" Keyword Issues

```
Wrong: Authorization: eyJhbGciOiJIUzI1NiIs...
Right: Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
	   ^^^^^^^
	   Required prefix
```

### Debug Checklist

```
+------------------------------------------+
|  JWT Debugging Steps                     |
+------------------------------------------+
|                                          |
|  1. Get token from login response        |
|                                          |
|  2. Decode at jwt.io (DON'T share real   |
|     tokens!)                             |
|                                          |
|  3. Check expiration claim (exp)         |
|                                          |
|  4. Verify issuer/audience match config  |
|                                          |
|  5. Check service logs for auth errors   |
|                                          |
|  6. Confirm header format is correct     |
|                                          |
+------------------------------------------+
```

---

## Quick Reference

### JWT Configuration (Same in ALL Services)

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

### Making Authenticated Requests

```bash
# 1. Login
TOKEN=$(curl -s -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Pass123!"}' \
  | jq -r '.data.token')

# 2. Use token
curl http://localhost:5000/api/hotels \
  -H "Authorization: Bearer $TOKEN"
```

---

*Document last updated: May 2024*
*For questions, contact the development team.*
