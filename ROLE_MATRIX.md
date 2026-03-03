# Role Matrix by UC/Controller

This file defines the intended authorization matrix for each UC/controller.
It is based on the current codebase plus the UC5/UC29 plan.

## Roles

- Admin: full access (role management, users, invoices, issues, reports, settings)
- Staff: view/manage invoices, process issues, update issue status
- Resident: view own invoices, submit issues, view announcements

## Current controllers

| UC/Feature | Controller/Action | Access | Notes |
| --- | --- | --- | --- |
| Authentication | AccountController.Login (GET/POST) | Anonymous | Public entry |
| Authentication | AccountController.Register (GET/POST) | Anonymous | Public entry |
| Authentication | AccountController.ForgotPassword (GET/POST) | Anonymous | Public entry |
| Authentication | AccountController.ResetPassword (GET/POST) | Anonymous | Public entry |
| Authentication | AccountController.AccessDenied (GET) | Anonymous | Public entry |
| Authentication | AccountController.PendingApproval (GET) | Anonymous | Public entry |
| Session | AccountController.Logout (POST) | Admin, Staff, Resident | Any authenticated |
| Profile | AccountController.Profile (GET/POST) | Admin, Staff, Resident | Any authenticated |
| Profile | AccountController.ChangePassword (GET/POST) | Admin, Staff, Resident | Any authenticated |
| Home | HomeController.Index | Admin, Staff, Resident | Intended dashboard |
| Home | HomeController.Privacy | Admin, Staff, Resident | Intended authenticated |
| Error | HomeController.Error | Anonymous | Safe to show |

## Planned controllers/UCs (from plan)

| UC/Feature | Controller/Action | Access | Notes |
| --- | --- | --- | --- |
| UC5 Role management | RolesController.Role CRUD | Admin | Create/update/delete roles |
| UC5 Role management | RolesController.AssignRole | Admin | Assign/remove roles to users |
| Invoices | InvoicesController.List/Create/Update | Admin, Staff | Staff/admin manage invoices |
| Invoices | InvoicesController.MyInvoices | Resident | Only own invoices |
| Issues | IssuesController.Create | Resident | Submit issues |
| Issues | IssuesController.List/UpdateStatus | Admin, Staff | Process and update issues |
| Issues | IssuesController.MyIssues | Resident | Only own issues |
| Announcements | AnnouncementsController.Index | Admin, Staff, Resident | Everyone can view |
| Announcements | AnnouncementsController.Create/Update | Admin, Staff | Management only |
