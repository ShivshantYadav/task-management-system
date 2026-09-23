# Team Task Management System

A role-based full-stack task management application built with **ASP.NET Core 8 Web API** and **React**. The system supports team management, task assignment, task tracking, comments, notifications, and role-based access control.

## Features

* JWT-based authentication
* BCrypt password hashing
* Role-based authorization
* Admin, Manager, and User roles
* Team creation and member management
* Task creation and assignment
* Task status tracking
* Task priority and due-date filtering
* Comments and collaboration
* Persistent task notifications
* Role-based dashboard
* Swagger / OpenAPI documentation
* Unit testing with xUnit
* Docker Compose
* GitHub Actions CI

## Roles & Permissions

| Role        | Responsibilities                                                              |
| ----------- | ----------------------------------------------------------------------------- |
| **Admin**   | Manage teams, assign managers, create users/managers, and manage tasks        |
| **Manager** | Manage their team members and create/update tasks for their team              |
| **User**    | View assigned tasks, update task status, add comments, and view notifications |

Public registration creates a **User** account only. Manager and Admin accounts are created through authorized endpoints.

## Technology Stack

| Area              | Technology                          |
| ----------------- | ----------------------------------- |
| Backend           | ASP.NET Core 8 Web API, C#          |
| Architecture      | Clean Architecture                  |
| Authentication    | JWT Bearer, BCrypt                  |
| Database          | SQL Server                          |
| ORM               | Entity Framework Core               |
| Frontend          | React 18, Vite, React Router, Axios |
| Testing           | xUnit, EF Core InMemory             |
| API Documentation | Swagger / OpenAPI                   |
| DevOps            | Docker Compose, GitHub Actions      |

## Architecture

The backend follows a Clean Architecture approach:

```text
Domain
   ↓
Application
   ↓
Infrastructure
   ↓
API
```

Responsibilities are separated across Domain entities/enums, application interfaces/business contracts, infrastructure services/data access, and API controllers/configuration.

## Project Structure

```text
TaskManagement/
├── TaskManagement.sln
├── TaskManagement.Domain/
├── TaskManagement.Application/
├── TaskManagement.Infrastructure/
│   ├── Data/
│   ├── Migrations/
│   ├── Security/
│   └── Services/
├── TaskManagement.API/
├── TaskManagement.Tests/
├── TaskManagement.Frontend/
├── docker-compose.yml
└── .github/
    └── workflows/
        └── ci.yml
```

## Main API Modules

### Authentication

```http
POST /api/auth/register
POST /api/auth/login
```

Registration creates a User account. Login returns a JWT with the user's identity, role, and expiration.

### Users

```http
GET  /api/users?role=Manager
GET  /api/users?role=User
POST /api/users
```

Admin users can create Manager or User accounts.

### Teams

```http
GET  /api/teams
GET  /api/teams/{id}
POST /api/teams
POST /api/teams/{id}/members
POST /api/teams/{id}/members/create
```

Teams can contain users and are managed according to the user's role.

### Tasks

```http
GET   /api/tasks
GET   /api/tasks/{id}
POST  /api/tasks
PUT   /api/tasks/{id}
PATCH /api/tasks/{id}/status
```

Supported statuses:

* To Do
* In Progress
* Done

Supported priorities:

* Low
* Medium
* High

Example filters:

```http
GET /api/tasks?status=InProgress
GET /api/tasks?priority=High
GET /api/tasks?dueAfter=2026-09-23
GET /api/tasks?dueBefore=2026-09-30
```

### Comments

```http
GET  /api/tasks/{taskId}/comments
POST /api/tasks/{taskId}/comments
```

Only users with access to a task can add comments.

### Notifications

```http
GET   /api/notifications
PATCH /api/notifications/{id}/read
```

Notifications are generated for important task events such as task assignment and status changes.

### Dashboard

```http
GET /api/dashboard/summary
```

The dashboard provides role-based task summaries and status information.

## Demo Credentials

The application seeds demo accounts when the database is empty.

| Email              | Password      | Role    |
| ------------------ | ------------- | ------- |
| `admin@demo.com`   | `Admin@123`   | Admin   |
| `manager@demo.com` | `Manager@123` | Manager |
| `user@demo.com`    | `User@123`    | User    |

A sample team and task are also created during initial database seeding.

## Getting Started

### Prerequisites

* .NET 8 SDK
* SQL Server or SQL Server LocalDB
* Node.js and npm
* `dotnet-ef`

### Backend Setup

From the project root:

```bash
dotnet restore
dotnet tool install --global dotnet-ef
dotnet ef database update --project TaskManagement.Infrastructure --startup-project TaskManagement.API
dotnet run --project TaskManagement.API
```

Swagger:

```text
https://localhost:5001/swagger
```

Use the HTTPS port displayed by the API if it differs from the example above.

### Frontend Setup

Open another terminal:

```bash
cd TaskManagement.Frontend
npm install
npm run dev
```

Frontend:

```text
http://localhost:5173
```

If required, configure the API URL in the frontend `.env` file using `.env.example` as a reference.

Example:

```env
VITE_API_URL=https://localhost:5001/api
```

## Docker

The project includes Docker Compose for the application services.

```bash
docker compose up --build
```

Default services:

```text
Frontend: http://localhost:3000
API:      http://localhost:5000
Swagger:  http://localhost:5000/swagger
SQL:      localhost:1433
```

## Testing

Run the test suite with:

```bash
dotnet test TaskManagement.sln
```

Tests cover authentication and important task business rules, including:

* Registration and duplicate email validation
* Login and password validation
* Inactive user handling
* Task assignment notifications
* Role-based authorization
* Team access validation
* Task status update rules

## API Documentation

Swagger/OpenAPI is available when the API is running:

```text
/swagger
```

A Postman collection is also included:

```text
docs/TeamTask.postman_collection.json
```

## CI/CD

GitHub Actions is configured to validate the project by:

* Restoring .NET dependencies
* Building the backend
* Running tests
* Installing frontend dependencies
* Building the React application

Workflow:

```text
.github/workflows/ci.yml
```

## Environment Configuration

Environment-specific values should be kept outside source control.

The frontend provides:

```text
.env.example
```

Do not commit real passwords, API keys, SMTP credentials, or other secrets.

## Repository

GitHub:

https://github.com/ShivshantYadav/task-management-system
