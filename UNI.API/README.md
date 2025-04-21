# UNI Framework API Solution

The UNI Framework API solution is a comprehensive .NET-based API framework designed to provide a robust foundation for building scalable and maintainable web APIs for UNI.

## Solution Structure

- **UNI.API** - Main API project containing core API implementation
- **UNI.API.Client** - Client library for consuming the API
- **UNI.API.Contracts** - Shared contracts and interfaces
- **UNI.API.Core** - Core business logic and services
- **UNI.API.DAL** - Data Access Layer for database operations
- **UNI.API.Deploy** - Deployment project and configuration
- **UNI.API.Tests** - Unit and integration tests
- **UNI.API.TestInstance** - Demo (and test) implementation

## Prerequisites

- .NET 6.0 SDK or later
- Visual Studio 2022 or Visual Studio Code
- MariaDB

## Getting Started

1. **Clone the Repository**
```bash
git clone https://github.com/orlodax/UNI-Framework.git
```

2. **Restore Dependencies**
```bash
dotnet restore UNI.API.sln
```

3. **Configure the Application**
   - Update connection strings in `appsettings.json`
   - Configure JWT settings if using authentication
   - Set up environment-specific settings

4. **Build the Solution**
```bash
dotnet build UNI.API.sln
```

5. **Run the Tests**
```bash
dotnet test UNI.API.sln
```

## Configuration

The framework uses a hierarchical configuration system:

```json
{
  "UNIClientConfiguration": {
    "ServerUrls": ["https://localhost:7297"],
    "ApiVersion": "v2",
    "IsTokenAutoRefreshable": true
  }
}
```

## Key Features

- **API Versioning** - Built-in support for API versioning
- **Authentication** - JWT-based authentication system
- **Swagger/OpenAPI** - Comprehensive API documentation
- **Error Handling** - Centralized error handling and logging
- **Database Access** - Flexible data access layer with MySQL support

## Package Versions

- Microsoft.AspNetCore.Authentication.JwtBearer: [6.0.14]
- Microsoft.AspNetCore.Mvc.Versioning: 5.0.0
- Swashbuckle.AspNetCore: 6.5.0
- MySqlConnector: 2.2.5

## Development Workflow

1. Create feature branch from `main`
2. Implement changes
3. Write/update tests
4. Create pull request
5. Code review
6. Merge to `main`

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License.

## Support

For support and questions:
1. Check the documentation
2. Open an issue in the GitHub repository
3. Contact the development team

---



## Links

- [Repository](https://github.com/orlodax/UNI-Framework)
- [Documentation](https://github.com/orlodax/UNI-Framework/wiki)
- [Issue Tracker](https://github.com/orlodax/UNI-Framework/issues)