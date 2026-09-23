# Assessment Business Flow

## 1. Authentication

- A new person registers through the public registration endpoint.
- Public registration always creates the `User` role.
- Login verifies the BCrypt password hash and returns a JWT with expiry.
- The frontend sends the JWT as `Authorization: Bearer <token>`.

## 2. Organization setup

1. Admin logs in.
2. Admin creates a team and selects a Manager.
3. Manager logs in and opens the team.
4. Manager adds User-role members to that team.

## 3. Task lifecycle

1. Admin or Manager selects a team.
2. Admin/Manager selects a team member as assignee.
3. Task is created with `ToDo` status and a priority/deadline.
4. Assignment creates a notification for the assignee.
5. Assignee changes status to `InProgress` and later `Done`.
6. A status change creates a notification for the relevant collaborator.
7. Authorized task participants can add comments.

## 4. Visibility and authorization

- Admin: organization-wide team/task visibility and management.
- Manager: own-team visibility and task/member management.
- User: assigned-task visibility, own task status updates, comments and notifications.
- API authorization is enforced server-side; frontend visibility is only a usability layer.

## 5. Dashboard

The dashboard is scoped by role and displays:
- To Do count
- In Progress count
- Done count
- Overdue count
- task status by assigned user
- upcoming tasks

The task page additionally supports filtering by status, priority, and deadline range.

## 6. Assessment walkthrough

The recommended demo starts with Admin, continues through Manager and User, and ends with Swagger, tests and Docker. This demonstrates the complete business lifecycle instead of showing isolated CRUD screens.
