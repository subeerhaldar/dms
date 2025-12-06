# Document Management System (DMS)

A SharePoint-like Document Management System built with .NET 8, ASP.NET Core Web API, and SQL Server 2022. This system provides comprehensive document storage, metadata management, workflow processing, permissions, and versioning capabilities.

## 🚀 Features

### Core Functionality
- **Document Storage**: Store files directly in SQL Server using VARBINARY(MAX)
- **Metadata Management**: Flexible key-value metadata system for documents
- **Workflow Management**: Basic approval workflows (Pending → Approved → Rejected)
- **Role-Based Permissions**: Admin, Editor, and Viewer roles with per-document ACLs
- **Version Control**: Automatic versioning with rollback capabilities
- **Full-Text Search**: Framework ready for SQL Server Full-Text Search
- **RESTful API**: Complete REST API with OpenAPI/Swagger documentation

### Technical Features
- **ASP.NET Core 8**: Latest .NET framework
- **Entity Framework Core**: ORM with migrations
- **ASP.NET Core Identity**: Authentication and authorization
- **JWT Tokens**: Secure API authentication
- **Swagger/OpenAPI**: Interactive API documentation
- **Clean Architecture**: Separated layers (API, Core, Infrastructure)

## 📋 Prerequisites

- .NET 8 SDK
- SQL Server 2022 (or compatible)
- Visual Studio 2022 or VS Code

## 🛠️ Setup and Installation

### 1. Clone and Build
```bash
git clone <repository-url>
cd DMS
dotnet build
```

### 2. Database Setup
Update the connection string in `Dms.Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=DmsDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Apply database migrations:
```bash
cd Dms.Api
dotnet ef database update
```

### 3. Run the Application
```bash
dotnet run
```

The application will start on `http://localhost:5000` (or the configured URL).

## 📖 API Documentation

### Swagger UI
Access the interactive API documentation at:
```
http://localhost:5000/swagger
```

The Swagger UI provides:
- Complete API endpoint documentation
- Interactive testing interface
- Request/response examples
- Authentication setup

### API Endpoints

#### Authentication
The API uses JWT Bearer tokens for authentication. Include the token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

#### Documents API

##### Get All Documents
```http
GET /api/Documents
Authorization: Bearer <token>
```

##### Get Document by ID
```http
GET /api/Documents/{id}
Authorization: Bearer <token>
```

##### Upload New Document
```http
POST /api/Documents
Content-Type: multipart/form-data
Authorization: Bearer <token>

Form Data:
- file: [binary file data]
- fileName: "example.pdf"
- workflowStateId: 1
```

##### Update Document
```http
PUT /api/Documents/{id}
Content-Type: multipart/form-data
Authorization: Bearer <token>

Form Data:
- file: [binary file data] (optional)
- fileName: "updated-name.pdf"
```

##### Delete Document
```http
DELETE /api/Documents/{id}
Authorization: Bearer <token>
```

##### Download Document
```http
GET /api/Documents/{id}/download
Authorization: Bearer <token>
```

#### Metadata API

##### Get Document Metadata
```http
GET /api/Documents/{id}/metadata
Authorization: Bearer <token>
```

##### Add Metadata
```http
POST /api/Documents/{id}/metadata
Content-Type: application/json
Authorization: Bearer <token>

{
  "key": "Department",
  "value": "IT"
}
```

#### Versioning API

##### Get Document Versions
```http
GET /api/Documents/{id}/versions
Authorization: Bearer <token>
```

##### Rollback to Version
```http
POST /api/Documents/{id}/rollback/{versionId}
Authorization: Bearer <token>
```

#### Permissions API

##### Get Document Permissions
```http
GET /api/Documents/{id}/permissions
Authorization: Bearer <token>
```

##### Add Permission
```http
POST /api/Documents/{id}/permissions
Content-Type: application/json
Authorization: Bearer <token>

{
  "userId": "user-guid",
  "role": "Editor"
}
```

## 🔐 Authentication

### JWT Configuration
The JWT settings are configured in `appsettings.json`:
```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyHere12345678901234567890",
    "Issuer": "DmsApi",
    "Audience": "DmsUsers"
  }
}
```

### User Roles
- **Admin**: Full access to all documents and permissions
- **Editor**: Can create, update, and manage documents
- **Viewer**: Read-only access to documents

## 🏗️ Architecture

### Project Structure
```
DMS/
├── Dms.Api/              # Web API project
│   ├── Controllers/      # API controllers
│   ├── Swagger/          # Swagger configuration
│   └── Program.cs        # Application entry point
├── Dms.Core/             # Domain layer
│   ├── Entities/         # Domain models
│   ├── Interfaces/       # Contracts
│   └── Services/         # Business logic
├── Dms.Infrastructure/   # Infrastructure layer
│   ├── Data/             # EF Core DbContext
│   └── Repositories/     # Data access implementations
└── README.md             # This file
```

### Design Patterns
- **Repository Pattern**: Data access abstraction
- **Service Layer**: Business logic encapsulation
- **Dependency Injection**: Loose coupling
- **Clean Architecture**: Separation of concerns

## 🗄️ Database Schema

### Main Tables
- **Documents**: File storage and metadata
- **DocumentVersions**: Version history
- **DocumentMetadata**: Key-value metadata
- **DocumentPermissions**: Access control
- **WorkflowStates**: Workflow definitions
- **AspNetUsers**: Identity users
- **AspNetRoles**: Identity roles

### Migrations
Database migrations are located in `Dms.Infrastructure/Migrations/`.

## 🧪 Testing the API

### Using Swagger UI
1. Start the application
2. Navigate to `http://localhost:5000/swagger`
3. Click "Authorize" and enter your JWT token
4. Test endpoints directly from the UI

### Using curl

#### Upload a file:
```bash
curl -X POST "http://localhost:5000/api/Documents" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "file=@example.pdf" \
  -F "fileName=example.pdf" \
  -F "workflowStateId=1"
```

#### Get documents:
```bash
curl -X GET "http://localhost:5000/api/Documents" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## 🔧 Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=DmsDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyHere12345678901234567890",
    "Issuer": "DmsApi",
    "Audience": "DmsUsers"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

## 🚀 Deployment

### Production Considerations
1. Update connection strings for production database
2. Configure proper JWT keys
3. Set up HTTPS
4. Configure logging and monitoring
5. Set up proper CORS policies if needed

### Docker Support
Add Dockerfile and docker-compose.yml for containerized deployment.

## 📝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License.

## 🆘 Support

For issues or questions:
1. Check the Swagger documentation
2. Review the code comments
3. Create an issue in the repository

---

**Built with .NET 8, ASP.NET Core, Entity Framework Core, and SQL Server**