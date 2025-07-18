# Products API

This project is part of a technical challenge focused to create a simple RESTful API built with .NET Core that demonstrates basic CRUD operations and JWT authentication.

## Features

- JWT-based authentication
- CRUD operations for products
- In-memory database (for development)
- Swagger/OpenAPI documentation
- Input validation
- Global error handling
- Structured logging

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Your favorite IDE

## Getting Started

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd ProductsChallenge
   ```

2. **Run the application**
   ```bash
   dotnet run --project Products
   ```

3. **Access the API**
   - Swagger UI: `https://localhost:5001/swagger`
   - Base URL: `https://localhost:5001/api`

## Authentication

1. **Get a JWT token**
   ```http
   POST /api/Auth/login
   Content-Type: application/json

   {
     "userName": "admin",
     "password": "admin123"
   }
   ```

2. **Use the token in requests**
   ```
   Authorization: Bearer <your-jwt-token>
   ```

## API Endpoints

### Products

- `GET /api/Products` - Get all products
- `GET /api/Products/{id}` - Get a product by ID
- `POST /api/Products` - Create a new product
- `PUT /api/Products/{id}` - Update a product
- `DELETE /api/Products/{id}` - Delete a product

## Configuration

Update `appsettings.json` to configure the application:

```json
{
  "JwtSettings": {
    "Secret": "your-256-bit-secret-key-here-make-it-very-long",
    "Issuer": "products-api",
    "Audience": "products-api-users",
    "ExpirationInMinutes": 120
  }
}
```

## Development

### Testing

Use Postman to test API endpoints.

Authenticate using the login endpoint (POST /api/auth/login) and copy the JWT token.

Include the JWT token in the authorization header (Bearer {token}) for product-related requests.


### Building for Production

```bash
dotnet publish -c Release -o ./publish
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
