# Team Task Management System

Role-based full-stack Task Management System built for the assessment brief. The application lets an organization manage teams, assign work, track task progress, collaborate through comments, and notify users of important task events.

## Assessment Business Flow

```text
1. User registers / logs in (self-registration always creates a plain "User")
        ↓
2. JWT is issued with UserId + Role + expiry
        ↓
3. Admin creates a team and assigns a Manager
        ↓
3b. Admin creates Manager/User accounts directly from the Users page (temp password emailed)
        ↓
4. Manager adds Users to the team - either an existing user, or by creating a brand-new
   employee account on the spot (temp password emailed to them)
        ↓
5. Admin or Manager creates a task for a team member
        ↓
6. Task starts as To Do
        ↓
7. Assignee works on the task → In Progress → Done
        ↓
8. Assignment and status changes create notifications (persisted + emailed when SMTP is configured)
        ↓
9. Team members with task access collaborate through comments
        ↓
10. Role-specific dashboards show task status and filters
```

## Roles and Responsibilities

| Role | Business responsibility |
|---|---|
| **Admin** | Creates/manages teams, assigns managers, creates Manager/User accounts directly (Users page), can add members, creates/updates/views all tasks. |
| **Manager** | Manages members of their own team — adds existing users **or creates brand-new employee accounts** — creates/updates tasks for their team, views team work. |
| **User** | Views assigned tasks, updates status of assigned tasks, comments, and reads notifications. |

**Security rule:** public registration (`/register`) always creates a plain `User` - there is no role field on that form and the API ignores/rejects any attempt to send one. Admin and Manager accounts, and any User accounts created on someone's behalf, are created through the authenticated, role-checked endpoints below - never through public self-registration.

### Creating accounts on someone else's behalf (Admin / Manager)

Two dedicated endpoints let privileged roles create accounts *for* someone else, instead of that person self-registering:

| Who | Endpoint | Creates | Notes |
|---|---|---|---|
| Admin | `POST /api/users` | A Manager or User account | Optional `teamId` immediately adds a new User to a team |
| Admin or the team's own Manager | `POST /api/teams/{id}/members/create` | A new User, added to that team | A Manager can only do this for their own team |

Both endpoints:
1. Generate a random temporary password (`TemporaryPasswordGenerator`) - never chosen by the admin/manager.
2. Hash it with BCrypt and save the new account exactly like any other user.
3. Email the new person their address + temporary password via `IEmailService` (see **Email / SMTP** below).
4. Return `{ user, emailSent, temporaryPassword }` - `temporaryPassword` is only included in the response when `emailSent` is `false`, so the admin/manager can still hand it over manually if no mail server is configured.

The new user is expected to log in once and change their password from **Profile**.

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 8 Web API, C# |
| Architecture | Clean Architecture: Domain / Application / Infrastructure / API |
| Authentication | JWT Bearer + BCrypt password hashing |
| Database | SQL Server + Entity Framework Core |
| Frontend | React 18 + Vite + React Router + Axios |
| Testing | xUnit + EF Core InMemory |
| API Docs | Swagger / OpenAPI |
| DevOps | Docker Compose + GitHub Actions |

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
└── .github/workflows/ci.yml
```

## Main API Flow

### Authentication

```text
POST /api/auth/register
POST /api/auth/login
```

Registration is User-only and requires `name`, `email`, `password`, `confirmPassword` (must match). Login returns a JWT with expiration.

### Users

```text
GET  /api/users?role=Manager
GET  /api/users?role=User
POST /api/users            (Admin only)
```

Admin can use the directory to select a Manager when creating a team. Managers can use it to select User-role members for their team. `POST /api/users` lets an Admin create a Manager or User account directly (see **Creating accounts on someone else's behalf** above); `role` must be `Manager` or `User` - `Admin` is rejected.

### Teams

```text
GET  /api/teams
GET  /api/teams/{id}
POST /api/teams
POST /api/teams/{id}/members          (add an existing user)
POST /api/teams/{id}/members/create   (create a brand-new user and add them)
```

Business rule:
- Admin creates a team and assigns a Manager.
- Manager can add existing Users to their own team, or create a brand-new employee account for their team in one step.
- Users can only see teams they belong to.

### Tasks

```text
GET   /api/tasks
GET   /api/tasks/{id}
POST  /api/tasks
PUT   /api/tasks/{id}
PATCH /api/tasks/{id}/status
```

Supported statuses:
- ToDo
- InProgress
- Done

Supported priorities:
- Low
- Medium
- High

Task list supports:

```text
?status=InProgress
?priority=High
?dueAfter=2026-09-23
?dueBefore=2026-09-30
```

### Comments

```text
GET  /api/tasks/{taskId}/comments
POST /api/tasks/{taskId}/comments
```

Only users who have access to the task can collaborate on it.

### Notifications

```text
GET   /api/notifications
PATCH /api/notifications/{id}/read
```

The assessment permits mock notifications instead of email. The system creates a database notification for:
- task assignment
- task status update

### Email / SMTP (account-creation emails)

When an Admin creates a user (`POST /api/users`) or a Manager creates a team member (`POST /api/teams/{id}/members/create`), the API always generates a temporary password and tries to email it via `IEmailService` / `EmailService` (`TaskManagement.Infrastructure/Services/EmailService.cs`), configured from the `Smtp` section of `appsettings.json`:

```json
"Smtp": {
  "Enabled": false,
  "Host": "smtp.gmail.com",
  "Port": 587,
  "EnableSsl": true,
  "Username": "",
  "Password": "",
  "FromEmail": "noreply@taskflow.local",
  "FromName": "TaskFlow"
}
```

- `Enabled` defaults to `false` so the project runs out of the box without any mail server. In that case the API logs what it *would* have sent and the `POST` response includes `temporaryPassword` so the admin/manager can share it manually.
- To send real emails, set `Enabled: true` and fill in `Host`/`Username`/`Password`/`FromEmail`. For Gmail, use an [App Password](https://myaccount.google.com/apppasswords) (not your normal password) with `Host: smtp.gmail.com`, `Port: 587`, `EnableSsl: true`. Any standard SMTP relay (SendGrid, Mailgun, Amazon SES SMTP, Outlook365, your company's mail server) works the same way.
- In Docker, set the equivalent environment variables instead of editing `appsettings.json`: `Smtp__Enabled`, `Smtp__Host`, `Smtp__Port`, `Smtp__EnableSsl`, `Smtp__Username`, `Smtp__Password`, `Smtp__FromEmail`, `Smtp__FromName` (already wired in `docker-compose.yml`, defaulting to disabled).
- A failed or disabled send never blocks account creation - the account is still created; only the email step is best-effort. `EmailSent: false` in the response is how the frontend knows to show the temporary password on-screen instead.

### Dashboard

```text
GET /api/dashboard/summary
```

The dashboard provides role-scoped task totals and status information. The task page provides filtering by deadline, status and priority.

## Demo Credentials

The API seeds these accounts on an empty database. Passwords are hashed with BCrypt at seed time.

| Email | Password | Role |
|---|---|---|
| `admin@demo.com` | `Admin@123` | Admin |
| `manager@demo.com` | `Manager@123` | Manager |
| `user@demo.com` | `User@123` | User |

The first run also creates a `Product Engineering` team, adds the demo User to it, and creates a sample task assigned to that User.

## Local Setup

### Backend

Prerequisites: .NET 8 SDK, SQL Server/LocalDB, and `dotnet-ef`.

```bash
cd TaskManagement
dotnet restore
 dotnet tool install --global dotnet-ef
 dotnet ef database update --project TaskManagement.Infrastructure --startup-project TaskManagement.API
 dotnet run --project TaskManagement.API
```

Swagger:

```text
https://localhost:5001/swagger
```

Use the actual HTTPS port printed by the API if it differs.

### Frontend

```bash
cd TaskManagement/TaskManagement.Frontend
npm install
npm run dev
```

Frontend:

```text
http://localhost:5173
```

Set `VITE_API_URL` in `.env` if the API uses a different URL.

## Docker

```bash
cd TaskManagement
docker compose up --build
```

Services:

```text
Frontend: http://localhost:3000
API:      http://localhost:5000
Swagger:  http://localhost:5000/swagger
SQL:      localhost:1433
```

The API waits for SQL Server, applies committed EF migrations, and then seeds demo data when the database is empty.

## Testing

```bash
cd TaskManagement
dotnet test TaskManagement.Tests/TaskManagement.Tests.csproj
```

Current tests cover authentication and important task business rules including assignment notifications and team-boundary validation.

## CI/CD

GitHub Actions validates:
- .NET restore/build/test
- React dependency installation/build

Deployment is an optional assessment bonus; the application is Docker-ready for deployment to a suitable .NET/SQL hosting environment.

## 5–8 Minute Walkthrough

Recommended recording order:

1. 0:00–0:30 — project purpose and architecture
2. 0:30–1:15 — Admin login, dashboard and team creation
3. 1:15–2:00 — Manager login and adding team members
4. 2:00–3:00 — Manager/Admin creates and assigns a task
5. 3:00–3:45 — User login, task details and status updates
6. 3:45–4:30 — Comments and notifications
7. 4:30–5:15 — Dashboard and deadline/status/priority filters
8. 5:15–6:00 — Swagger API and JWT authorization
9. 6:00–6:40 — Unit tests
10. 6:40–7:30 — Docker Compose and GitHub Actions
11. 7:30–8:00 — README and final project structure

## Assessment Coverage

- Backend API design and authentication/authorization
- SQL Server relational design with EF Core
- React + Axios responsive UI
- Admin / Manager / User business rules
- Validation and centralized API error handling
- Unit tests
- Docker multi-container setup
- GitHub Actions CI
- Swagger documentation
- README and walkthrough flow

## Test Coverage

11 xUnit tests (EF Core InMemory) covering:

- Registration, duplicate email, weak password, login (valid / wrong password / inactive user)
- Task creation notification trigger (mock notification persisted)
- RBAC rules: User cannot create tasks, Manager cannot create tasks for another manager's team,
  non-assignee cannot update task status
- Status change flow: ToDo -> InProgress with TaskStatusUpdated notification to the task creator

Run tests: `dotnet test TaskManagement.sln`

## Deliverables Checklist

- [x] GitHub repository (frontend + backend, organized in Clean Architecture layers)
- [x] README with setup instructions, sample credentials, tech stack
- [x] API documentation: Swagger (`/swagger`) + Postman collection in `docs/TeamTask.postman_collection.json`
- [x] Dockerized multi-container setup: `docker compose up --build`
- [x] CI/CD: GitHub Actions (`.github/workflows/ci.yml`) - restore, build, test, frontend build
- [ ] **Video walkthrough (5-8 min): [ADD LOOM / DRIVE LINK HERE]**
      Suggested script: Admin creates team -> Manager adds member + assigns task -> User moves
      task ToDo -> InProgress -> Done (notifications shown) -> comment on task -> dashboard +
      filters -> Swagger tour -> docker compose demo.
