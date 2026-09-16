# Production Planning System

A full-stack **Production Planning Management System** designed to manage and organize production planning data through a clean, responsive web interface.

The project combines an **Angular frontend**, a **.NET Web API backend**, and a **SQL Server database**, demonstrating a complete full-stack application architecture with frontend–backend communication, API integration, database connectivity, and structured data management.

---

## 📌 About the Project

The Production Planning System was developed to provide a structured interface for managing production planning information.

The application allows users to interact with production-related records through an organized dashboard and data table rather than working directly with raw database information.

The project focuses on:

* Full-stack application development
* REST API integration
* Database connectivity
* Structured data management
* Responsive user interface design
* Separation of frontend and backend architecture

---

## ✨ Features

### 📊 Production Planning Dashboard

The application provides a dedicated **Production Planning Standard** interface where production-related information can be displayed and managed in an organized format.

### 📋 Data Table

Production planning records are displayed through a structured table interface, making information easier to read, browse, and manage.

The table-based design is built using reusable frontend components and is suitable for handling larger datasets.

### 🔗 Frontend & Backend Integration

The Angular frontend communicates with the .NET backend through REST APIs.

This keeps the presentation layer separate from the business and data-access layers and makes the application easier to maintain and extend.

### 🗄️ Database Integration

The backend is connected to **Microsoft SQL Server** for persistent data storage.

**Entity Framework Core** is used to communicate with the database, manage entities, and handle database operations.

### 🌐 REST API

The backend exposes API endpoints that can be consumed by the Angular frontend.

The API architecture provides a foundation for operations such as:

* Retrieving records
* Adding new records
* Updating existing information
* Deleting records
* Connecting production data with the frontend interface

### 📱 Responsive Interface

The frontend is designed to work across different screen sizes while maintaining a clean and structured layout.

### 🎨 Component-Based UI

The Angular application uses reusable components and **PrimeNG** UI elements to create a consistent interface, including tables and navigation elements.

---

# 🛠️ Tech Stack

## Frontend

* Angular
* TypeScript
* HTML5
* CSS3
* PrimeNG

## Backend

* .NET Web API
* C#
* ASP.NET Core
* Entity Framework Core
* REST APIs

## Database

* Microsoft SQL Server
* SQL Server running through Docker

## Development Tools

* Visual Studio Code
* Docker Desktop
* Git
* GitHub
* Entity Framework Core CLI

---

# 🏗️ System Architecture

The project follows a separated full-stack architecture:

```text
                USER
                  │
                  ▼
        ┌───────────────────┐
        │ Angular Frontend  │
        │                   │
        │ UI / Components   │
        │ PrimeNG Tables    │
        └─────────┬─────────┘
                  │
                  │ HTTP / REST API
                  ▼
        ┌───────────────────┐
        │   .NET Web API    │
        │                   │
        │ Controllers       │
        │ Business Logic    │
        │ EF Core           │
        └─────────┬─────────┘
                  │
                  ▼
        ┌───────────────────┐
        │    SQL Server     │
        │                   │
        │ Persistent Data   │
        └───────────────────┘
```

The Angular application handles the user interface, while the .NET Web API processes requests and communicates with SQL Server through Entity Framework Core.

---

# 📁 Project Structure

```text
Production-Planning-System/
│
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/
│   │   │   ├── pages/
│   │   │   └── services/
│   │   │
│   │   ├── assets/
│   │   └── styles/
│   │
│   ├── angular.json
│   └── package.json
│
├── backend/
│   ├── Controllers/
│   ├── Data/
│   ├── Models/
│   ├── Properties/
│   ├── Program.cs
│   └── appsettings.json
│
├── README.md
└── .gitignore
```

---

# 🔄 How the Application Works

When a user interacts with the Angular interface, the frontend sends an HTTP request to the backend API.

```text
User Action
     ↓
Angular Component
     ↓
Angular Service
     ↓
HTTP Request
     ↓
.NET API
     ↓
Entity Framework Core
     ↓
SQL Server
```

The database returns the requested information to the backend.

The backend then sends a response to Angular, where the information is displayed to the user.

---

# 🚀 Running the Project Locally

## Prerequisites

Make sure the following are installed:

* Node.js
* Angular CLI
* .NET SDK
* Docker Desktop
* Git

---

## 1. Clone the Repository

```bash
git clone <your-repository-url>

cd Production-Planning-System
```

---

## 2. Start SQL Server

Make sure Docker Desktop is running.

Check the SQL Server container:

```bash
docker ps
```

The SQL Server container should be running on:

```text
localhost:1433
```

---

## 3. Run the Backend

Navigate to the backend:

```bash
cd backend
```

Restore dependencies:

```bash
dotnet restore
```

Run the backend:

```bash
dotnet run
```

During local development, the API may run on an address similar to:

```text
http://localhost:5271
```

Use the URL shown in your terminal if the assigned development port is different.

---

## 4. Run the Frontend

Open another terminal and navigate to:

```bash
cd frontend
```

Install dependencies:

```bash
npm install
```

Start Angular:

```bash
ng serve
```

Then open:

```text
http://localhost:4200
```

---

# 🔌 API Integration

Angular services are responsible for communicating with the backend.

A typical request flow is:

```text
Angular
   ↓
HTTP Request
   ↓
.NET Controller
   ↓
Entity Framework Core
   ↓
SQL Server
   ↓
API Response
   ↓
Angular UI
```

This architecture keeps the frontend, backend, and database responsibilities separated.

---

# 🗃️ Database

Microsoft SQL Server is used as the relational database.

The SQL Server instance can run inside a Docker container, allowing the development database environment to remain isolated from the local operating system.

Entity Framework Core acts as the bridge between the .NET application and SQL Server.

Database migrations can be applied using:

```bash
dotnet ef database update
```

---

# 🎯 Project Objectives

This project was built to gain practical experience with:

* Building a complete full-stack application
* Developing REST APIs with ASP.NET Core
* Connecting Angular with a .NET backend
* Working with SQL Server
* Using Entity Framework Core
* Managing relational application data
* Building responsive Angular interfaces
* Using PrimeNG components
* Understanding frontend/backend separation
* Working with Docker-based databases
* Managing source code with Git and GitHub

---

# 🔮 Future Improvements

The system can be expanded with additional production-management functionality, including:

* User authentication and authorization
* Role-based access control
* Advanced CRUD operations
* Production scheduling
* Inventory integration
* Search and filtering
* Sorting and pagination
* Production status tracking
* Reporting dashboards
* Data visualization
* Notifications
* Export to Excel/PDF
* Deployment to a cloud environment

---

# 💡 What I Learned

Building this project helped me understand how the different layers of a full-stack application work together.

Rather than creating only a frontend interface, I worked with the complete application flow:

**Angular → REST API → .NET → Entity Framework Core → SQL Server**

The project also provided practical experience with database configuration, API integration, Docker, responsive frontend development, debugging, and Git/GitHub workflows.

---

# 📄 License

This project is intended for educational and portfolio purposes.

---

## ⭐ Production Planning System

A practical full-stack project demonstrating the integration of **Angular, ASP.NET Core Web API, Entity Framework Core, SQL Server, PrimeNG, and Docker** in a structured production planning application.

