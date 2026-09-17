# Production Planning System

A full-stack production planning application built with **Angular, ASP.NET Core Web API, Entity Framework Core, and Microsoft SQL Server**.

The system provides a structured interface for managing production-related data with configurable data views, filtering, sorting, pagination, persistent user preferences, and RESTful backend integration.

---

## Overview

Production Planning System is designed around a modular full-stack architecture that separates the presentation, application, and persistence layers.

The Angular frontend provides a responsive data-management interface, while the ASP.NET Core backend exposes REST APIs responsible for application logic and database communication. Microsoft SQL Server is used for persistent storage, with Entity Framework Core providing ORM and migration support.

The repository contains both frontend and backend applications within a single project structure.

## Core Features

* Production data management
* Responsive data-grid interface
* Search and dynamic filtering
* Multi-field sorting
* Pagination and configurable page size
* Configurable table columns
* Field Chooser with Select All and Clear controls
* Persistent UI preferences using browser storage
* Table configuration reset
* RESTful API architecture
* SQL Server persistence
* Entity Framework Core migrations
* JWT authentication configuration
* Frontend/backend CORS configuration
* Responsive desktop and mobile layouts

## Technology Stack

| Layer                | Technologies                        |
| -------------------- | ----------------------------------- |
| Frontend             | Angular 21, TypeScript, HTML5, CSS3 |
| UI                   | PrimeNG, Angular CDK                |
| Backend              | ASP.NET Core Web API, .NET 10, C#   |
| ORM                  | Entity Framework Core               |
| Database             | Microsoft SQL Server 2022           |
| Authentication       | JWT Bearer Authentication           |
| API Documentation    | Swagger / OpenAPI                   |
| Database Environment | Docker                              |
| Development          | VS Code, DBeaver                    |
| Version Control      | Git, GitHub                         |

## Architecture

The application follows a conventional client-server architecture:

```text
┌──────────────────────────────┐
│       Angular Frontend       │
│                              │
│  Components • Services • UI  │
└──────────────┬───────────────┘
               │
               │ HTTP / REST
               ▼
┌──────────────────────────────┐
│     ASP.NET Core Web API     │
│                              │
│ Controllers • Business Logic │
└──────────────┬───────────────┘
               │
               │ Entity Framework Core
               ▼
┌──────────────────────────────┐
│     Microsoft SQL Server     │
│                              │
│       Persistent Data        │
└──────────────────────────────┘
```

## Repository Structure

```text
Production-Planning-System/
│
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   ├── assets/
│   │   └── ...
│   ├── angular.json
│   └── package.json
│
├── backend/
│   ├── Controllers/
│   ├── Data/
│   ├── Models/
│   ├── Migrations/
│   ├── Properties/
│   ├── Program.cs
│   └── ...
│
├── .gitignore
└── README.md
```

## Frontend

The frontend is developed with **Angular 21** and provides the primary interface for interacting with production data.

PrimeNG components are used alongside custom Angular and CSS implementations to provide a responsive enterprise-style interface.

### Data Grid

The primary data grid supports:

* Searching
* Filtering
* Sorting
* Pagination
* Configurable page size
* Dynamic field visibility
* Persistent user preferences

### Field Chooser

Users can customize the information displayed in the grid by selecting individual fields.

The Field Chooser supports:

* Individual field selection
* Select All
* Clear All
* Apply changes
* Persistent configuration
* Reset to default configuration

Selected table preferences are stored locally so the interface can retain its configuration between sessions.

## Backend

The backend is implemented using **ASP.NET Core Web API on .NET 10**.

It provides the application API layer and manages communication between the Angular client and SQL Server database.

The backend architecture includes:

* REST API endpoints
* Entity Framework Core integration
* Database migrations
* Dependency injection
* JWT Bearer authentication configuration
* CORS configuration
* OpenAPI documentation
* Environment-specific configuration

## Database

**Microsoft SQL Server 2022** is used as the relational database management system.

The local development database runs inside Docker and is exposed through port `1433`.

Entity Framework Core handles database access and schema migrations.

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

Database inspection and management can also be performed using DBeaver or another SQL Server-compatible database client.

## API

During local development, the backend API runs at:

```text
http://localhost:5271
```

The Angular development server runs at:

```text
http://localhost:4200
```

CORS is configured to allow communication between the Angular client and ASP.NET Core API during development.

## Local Development

### Prerequisites

Install the following before running the project:

* Node.js 20+
* Angular CLI
* .NET 10 SDK
* Docker
* Git
* SQL Server-compatible database client (optional)

### Clone

```bash
git clone https://github.com/Mariam-N1/Production-Planning-System.git
cd Production-Planning-System
```

### Database

Start the SQL Server Docker container:

```bash
docker start sqlserver
```

Verify its status:

```bash
docker ps
```

### Backend

```bash
cd backend
dotnet restore
dotnet ef database update
dotnet run
```

The API should become available at:

```text
http://localhost:5271
```

### Frontend

From a separate terminal:

```bash
cd frontend
npm install
ng serve
```

Open:

```text
http://localhost:4200
```

## Configuration

Sensitive application configuration should remain outside source control.

The backend supports .NET User Secrets for development configuration, including:

```text
ConnectionStrings:DefaultConnection
Jwt:Key
```

Example:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<connection-string>"
dotnet user-secrets set "Jwt:Key" "<secure-jwt-key>"
```

Production deployments should use environment variables or an appropriate secrets-management solution.

## API Development

Swagger/OpenAPI support is available in the development environment for API inspection and endpoint testing.

This allows backend endpoints to be tested independently before integration with the Angular client.

## Planned Development

The architecture is designed to support additional production-planning capabilities, including:

* Role-based authorization
* Production scheduling
* Material and inventory planning
* Production status workflows
* Reporting and analytics
* KPI dashboards
* Data export
* Audit logging
* Notification workflows
* Automated testing
* Cloud deployment

## Engineering Considerations

The project emphasizes:

**Separation of concerns** — frontend, API, and persistence responsibilities are maintained independently.

**Persistence** — production data is stored in SQL Server rather than relying solely on client-side state.

**Configuration security** — credentials and authentication secrets remain outside the repository.

**Maintainability** — the project structure allows frontend and backend functionality to evolve independently.

**Responsive design** — data-management functionality remains accessible across different viewport sizes.

**Extensibility** — the architecture provides a foundation for additional planning, reporting, authentication, and workflow capabilities.

---

## Author

**Hafiza Mariam Nadeem**

Computer Science
Full-Stack Development

GitHub: `Mariam-N1`

---

**Status:** Active Development
