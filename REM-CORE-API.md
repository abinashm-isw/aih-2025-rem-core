# REM Core API Documentation

## Overview

The REM (Real Estate Management) Core API is a RESTful web service built with ASP.NET Core 8.0 that provides contract management functionality for real estate operations. The API uses Oracle Database for data persistence and follows standard REST conventions.

**Base URL:** `http://localhost:5180` (Development)  
**API Version:** v1  
**Database:** Oracle Database  

## Table of Contents

- [Authentication](#authentication)
- [API Endpoints](#api-endpoints)
  - [Health Check Endpoints](#health-check-endpoints)
  - [Contract Management Endpoints](#contract-management-endpoints)
- [Data Models](#data-models)
- [Error Handling](#error-handling)
- [Configuration](#configuration)
- [Development Setup](#development-setup)

## Authentication

Currently, the API does not implement authentication. All endpoints are publicly accessible. This may change in future versions.

## API Endpoints

### Health Check Endpoints

#### Ping Check
**GET** `/api/health/ping`

Basic health check endpoint to verify the API is running.

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2025-05-28T10:30:00.000Z"
}
```

#### Database Connection Check
**GET** `/api/health/database`

Checks the database connectivity and returns the connection status.

**Response (Success):**
```json
{
  "status": "healthy",
  "message": "Database connection successful"
}
```

**Response (Failure):**
```json
{
  "status": "unhealthy",
  "message": "Database connection failed",
  "error": "Oracle connection error details"
}
```

### Contract Management Endpoints

#### Get All Contracts
**GET** `/api/contracts`

Retrieves all active contracts (excluding archived ones by default).

**Response:**
```json
[
  {
    "id": 1,
    "contracttypeid": 2,
    "description": "Office Lease Agreement",
    "vendorid": 100,
    "contractedpartyid": 200,
    "currencyid": 1,
    "isreceivable": true,
    "isarchived": false,
    "status": "Active",
    "referenceno": "REF-2025-001",
    // ... additional fields
  }
]
```

**Status Codes:**
- `200 OK` - Success
- `500 Internal Server Error` - Server error

#### Get Contract by ID
**GET** `/api/contracts/{id}`

Retrieves a specific contract by its ID.

**Parameters:**
- `id` (path) - Contract ID (integer)

**Response:**
```json
{
  "id": 1,
  "contracttypeid": 2,
  "description": "Office Lease Agreement",
  "vendorid": 100,
  "contractedpartyid": 200,
  "currencyid": 1,
  "isreceivable": true,
  "isarchived": false,
  "status": "Active",
  "referenceno": "REF-2025-001",
  // ... additional fields
}
```

**Status Codes:**
- `200 OK` - Success
- `404 Not Found` - Contract not found
- `500 Internal Server Error` - Server error

#### Create Contract
**POST** `/api/contracts`

Creates a new contract.

**Request Body:**
```json
{
  "contracttypeid": 2,
  "description": "New Office Lease",
  "vendorid": 100,
  "contractedpartyid": 200,
  "currencyid": 1,
  "isreceivable": true,
  "referenceno": "REF-2025-002",
  "status": "Active",
  "notes": "Initial contract setup"
}
```

**Response:**
```json
{
  "id": 2,
  "contracttypeid": 2,
  "description": "New Office Lease",
  "vendorid": 100,
  "contractedpartyid": 200,
  "currencyid": 1,
  "isreceivable": true,
  "isarchived": false,
  "status": "Active",
  "referenceno": "REF-2025-002",
  "entityid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  // ... additional fields
}
```

**Status Codes:**
- `201 Created` - Contract created successfully
- `400 Bad Request` - Invalid request data
- `500 Internal Server Error` - Server error

#### Update Contract
**PUT** `/api/contracts/{id}`

Updates an existing contract.

**Parameters:**
- `id` (path) - Contract ID (integer)

**Request Body:**
```json
{
  "contracttypeid": 2,
  "description": "Updated Office Lease",
  "vendorid": 100,
  "contractedpartyid": 200,
  "currencyid": 1,
  "isreceivable": false,
  "isarchived": false,
  "referenceno": "REF-2025-001-UPD",
  "status": "Modified",
  "notes": "Updated contract terms"
}
```

**Response:**
```json
{
  "id": 1,
  "contracttypeid": 2,
  "description": "Updated Office Lease",
  "vendorid": 100,
  "contractedpartyid": 200,
  "currencyid": 1,
  "isreceivable": false,
  "isarchived": false,
  "status": "Modified",
  "referenceno": "REF-2025-001-UPD",
  // ... additional fields
}
```

**Status Codes:**
- `200 OK` - Contract updated successfully
- `400 Bad Request` - Invalid request data
- `404 Not Found` - Contract not found
- `500 Internal Server Error` - Server error

#### Delete Contract (Archive)
**DELETE** `/api/contracts/{id}`

Soft deletes a contract by archiving it.

**Parameters:**
- `id` (path) - Contract ID (integer)

**Status Codes:**
- `204 No Content` - Contract archived successfully
- `404 Not Found` - Contract not found
- `500 Internal Server Error` - Server error

#### Search Contracts
**GET** `/api/contracts/search`

Searches contracts with various filters.

**Query Parameters:**
- `description` (string, optional) - Filter by description (contains)
- `status` (string, optional) - Filter by status (exact match)
- `vendorId` (integer, optional) - Filter by vendor ID
- `contractTypeId` (integer, optional) - Filter by contract type ID

**Example:**
```
GET /api/contracts/search?description=office&status=Active&vendorId=100
```

**Response:**
```json
[
  {
    "id": 1,
    "description": "Office Lease Agreement",
    "status": "Active",
    "vendorid": 100,
    // ... additional fields
  }
]
```

**Status Codes:**
- `200 OK` - Success
- `500 Internal Server Error` - Server error

#### Get Contract Statistics
**GET** `/api/contracts/stats`

Retrieves basic statistics about contracts (health check endpoint).

**Response:**
```json
{
  "totalActiveContracts": 150,
  "contractsByStatus": {
    "Active": 120,
    "Pending": 20,
    "Terminated": 10
  },
  "recentContracts": [
    {
      "id": 1,
      "description": "Office Lease Agreement",
      "status": "Active"
    },
    // ... up to 5 most recent contracts
  ]
}
```

**Status Codes:**
- `200 OK` - Success
- `500 Internal Server Error` - Server error

## Data Models

### ContractDto
Full contract data transfer object returned by GET operations.

```json
{
  "id": "integer",
  "contracttypeid": "integer|null",
  "description": "string|null",
  "vendorid": "integer|null",
  "contractedpartyid": "integer|null",
  "currencyid": "integer|null",
  "isreceivable": "boolean|null",
  "isarchived": "boolean|null",
  "isinholdover": "boolean|null",
  "entityid": "guid|null",
  "discriminator": "string|null",
  "isbroken": "boolean|null",
  "netequivalentfactor": "decimal|null",
  "leaseaccountingOriginalpurchaseprice": "decimal|null",
  "leaseaccountingEoltakeownership": "boolean|null",
  "leaseaccountingInitialprepayment": "decimal|null",
  "leaseaccountingUsefullife": "integer|null",
  "leaseaccountingCalculatedrestoringrate": "decimal|null",
  "leaseaccountingLeasetype": "string|null",
  "leaseaccountingAssetcategorytype": "string|null",
  "leaseaccountingLedgersystem": "string|null",
  "makegooddateofobligation": "datetime|null",
  "leaseaccountingStartdate": "datetime|null",
  "leaseaccountingManualoverride": "integer|null",
  "archiveddate": "datetime|null",
  "holdoverstartdate": "datetime|null",
  "leaseaccountingForcereview": "boolean|null",
  "treasuryapproverid": "integer|null",
  "ispartialbuilding": "boolean|null",
  "lifecycleState": "string|null",
  "clonedfromcontractid": "integer|null",
  "leaseaccountingAccountingcode": "string|null",
  "notes": "string|null",
  "referenceno": "string|null",
  "status": "string|null",
  "terminationcost": "decimal|null",
  "terminationdate": "datetime|null"
}
```

### CreateContractDto
Data required to create a new contract.

```json
{
  "contracttypeid": "integer|null",
  "description": "string|null",
  "vendorid": "integer|null",
  "contractedpartyid": "integer|null",
  "currencyid": "integer|null",
  "isreceivable": "boolean|null",
  "referenceno": "string|null",
  "status": "string|null",
  "notes": "string|null"
}
```

### UpdateContractDto
Data for updating an existing contract.

```json
{
  "contracttypeid": "integer|null",
  "description": "string|null",
  "vendorid": "integer|null",
  "contractedpartyid": "integer|null",
  "currencyid": "integer|null",
  "isreceivable": "boolean|null",
  "isarchived": "boolean|null",
  "referenceno": "string|null",
  "status": "string|null",
  "notes": "string|null"
}
```

## Error Handling

The API uses standard HTTP status codes and returns error information in JSON format.

### Error Response Format
```json
{
  "title": "Error Title",
  "status": 400,
  "detail": "Detailed error message",
  "instance": "/api/contracts/123"
}
```

### Common Status Codes
- `200 OK` - Request successful
- `201 Created` - Resource created successfully
- `204 No Content` - Request successful, no content to return
- `400 Bad Request` - Invalid request data
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error
- `503 Service Unavailable` - Database connection issues

## Configuration

### Connection String
The API connects to Oracle Database using the connection string configured in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "OracleConnection": "Data Source=host:port/service;User Id=username;Password=password;Persist Security Info=false;"
  }
}
```

### Environment Configuration
- **Development:** Swagger UI enabled at root path (`/`)
- **Production:** Swagger UI disabled for security

### CORS Configuration
CORS is configured to allow all origins, methods, and headers for MCP (Model Context Protocol) server integration.

## Development Setup

### Prerequisites
- .NET 8.0 SDK
- Oracle Database access
- Visual Studio 2022 or VS Code

### Running the Application

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd aih-2025-rem-core/RemCoreApi
   ```

2. **Configure the database connection:**
   Update the connection string in `appsettings.json` or `appsettings.Development.json`

3. **Install dependencies:**
   ```bash
   dotnet restore
   ```

4. **Run the application:**
   ```bash
   dotnet run
   ```

5. **Access Swagger UI:**
   Navigate to `https://localhost:5001` in your browser

### Database Schema
The API uses the following Oracle database schema:
- **Schema:** `DEV_RAY2__REM`
- **Main Table:** `CONTRACTS_CONTRACT`

### Boolean Handling
The API handles Oracle's NUMBER(1) boolean fields with special consideration for Entity Framework Core compatibility:
- Database: `1` (true) / `0` (false) / `NULL` stored as Oracle NUMBER(1) fields
- API: `true` / `false` / `null`

**Technical Implementation:**
- Boolean fields are mapped as integers in Entity Framework to avoid Oracle EF Core casting issues
- Raw SQL queries are used for complex operations to bypass problematic boolean type mapping
- The `AsNoTracking()` method is used for read-only operations to improve performance
- Type conversion is handled automatically in the service layer mapping methods

**Known Oracle EF Core Issues:**
- Oracle EF Core has limitations with boolean literal casting in LINQ queries
- NUMBER(1) fields require special handling to prevent SQL generation errors
- Raw SQL queries provide better compatibility for complex boolean filtering operations

## Future Enhancements

- Authentication and authorization
- API versioning
- Rate limiting
- Caching
- Additional contract-related endpoints
- Integration with MCP servers
- Real-time notifications
- Audit logging

## Support

For technical support or questions about the API, please contact the development team or refer to the project documentation.

---

**Last Updated:** May 28, 2025  
**API Version:** v1.0.0
