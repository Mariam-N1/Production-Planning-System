# Production Planning

A production planning app for a paint company. Products are defined once,
linked to the shades and pack sizes they come in, and production runs are
raised, approved and marked as made.

- **Backend** — ASP.NET Core 10 Web API, Entity Framework Core, SQL Server
- **Frontend** — Angular 21, PrimeNG
- **Auth** — JWT, hashed passwords, roles, account lockout

---

## What's in it

| Screen | Does |
|---|---|
| Overview | live counts, most-used shades, what needs attention |
| Products | CRUD, with shades and pack sizes linked to each product |
| Shades | CRUD, unique hex codes |
| Packing | CRUD, sizes in litres |
| Approvals | production runs: raise → approve → mark as made |
| Settings | profile, password, light/dark theme, delete account |

---

## Running it

### 1. SQL Server

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<your password>" \
  -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

### 2. Secrets

Nothing sensitive is committed. The connection string and the JWT signing
key come from user-secrets, so set them once:

```bash
cd BackendProject
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Server=localhost,1433;Database=CrudDb;User Id=sa;Password=<your password>;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:Key" "<any random string, 32 characters or more>"
```

The app prints these instructions and stops if either is missing.

### 3. Database

```bash
dotnet ef database update
```

### 4. Run

```bash
dotnet run        # API + Swagger on http://localhost:5271
```

```bash
cd screen
npm install
ng serve          # app on http://localhost:4200
```

The **first account you sign up** becomes the Admin — only Admins can
approve production runs.

---

## Notes

**Email is not wired up.** Password reset codes are written to the terminal
running `dotnet run` instead of being emailed. `Services/EmailSender.cs`
already contains the SMTP path; it falls back to logging while
`Smtp:User` and `Smtp:Password` are unset.

**Roles.** `User` can raise production runs; `Admin` can approve, reject and
mark them as made. Enforced by `[Authorize(Roles = "Admin")]` on the API —
the hidden buttons in the UI are only a convenience.

**Lockout.** Five wrong passwords locks an account for 15 minutes.
