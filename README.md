# JWT Auth Solution — Clean Architecture

## Сохтори проект

```
Solution/
├── Domain/
│   ├── Entities/
│   │   ├── AppUser.cs          ← IdentityUser + FirstName, LastName, IsActive
│   │   └── AppRole.cs          ← IdentityRole + Description
│   └── Enums/
│       └── UserRole.cs
│
├── Application/
│   ├── DTOs/
│   │   └── Dtos.cs             ← LoginDto, RegisterDto, UserDto, ...
│   ├── Interfaces/
│   │   └── IServices.cs        ← IJwtService, IEmailService, IUserService
│   ├── Results/
│   │   └── Result.cs           ← Result<T> pattern
│   └── Pagination/
│       └── PagedResult.cs      ← PaginationParams, PagedResult<T>
│
├── Infrastructure/
│   ├── Data/
│   │   └── AppDbContext.cs     ← IdentityDbContext<AppUser, AppRole>
│   ├── Configurations/
│   │   ├── AppUserConfiguration.cs
│   │   └── AppRoleConfiguration.cs
│   ├── Identity/
│   │   └── RoleConstants.cs    ← Admin | Manager | User
│   └── Services/
│       ├── JwtService.cs
│       ├── EmailService.cs
│       └── UserService.cs
│
└── API/
    ├── Controllers/
    │   ├── AuthController.cs   ← register / login / change-password / me
    │   └── UsersController.cs  ← CRUD + assign/remove role
    ├── Middleware/
    │   └── ExceptionMiddleware.cs
    ├── Program.cs
    └── appsettings.json
```

---

## Оғоз кардан

### 1. PostgreSQL насб кунед ва пайвандро дар appsettings.json танзим кунед:
```json
"ConnectionStrings": {
  "Default": "Host=localhost;Port=5432;Database=JwtAuthDb;Username=postgres;Password=yourpassword"
}
```

### 2. Migration сохтан ва татбик кардан:
```bash
cd Solution

dotnet ef migrations add InitialCreate --project Infrastructure --startup-project API
dotnet ef database update --project Infrastructure --startup-project API
```

### 3. Иҷро кардан:
```bash
cd API
dotnet run
```

Swagger: **http://localhost:5000**

---

## API Endpoints

### Бидуни авторизатсия:
| Method | URL | Тавсиф |
|--------|-----|--------|
| POST | `/api/auth/register` | Бакайдгирии корбари нав |
| POST | `/api/auth/login` | Воридшавӣ + JWT token |

### Бо токен (ҳама корбарон):
| Method | URL | Тавсиф |
|--------|-----|--------|
| GET | `/api/auth/me` | Маълумоти корбари кунунӣ |
| POST | `/api/auth/change-password` | Иваз кардани парол |

### Танхо Admin:
| Method | URL | Тавсиф |
|--------|-----|--------|
| GET | `/api/users` | Рӯйхати корбарон (pagination) |
| GET | `/api/users/{id}` | Корбар бо ID |
| PUT | `/api/users/{id}` | Навсозии корбар |
| DELETE | `/api/users/{id}` | Гайрифаъол кардан (soft delete) |
| POST | `/api/users/assign-role` | Таъини рол |
| POST | `/api/users/remove-role` | Гирифтани рол |

---

## Pagination мисол:
```
GET /api/users?pageNumber=1&pageSize=10
```

Чавоб:
```json
{
  "items": [...],
  "totalCount": 50,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5,
  "hasPrevious": false,
  "hasNext": true
}
```

---

## Logging сатхҳо

| Сатх | Вақт |
|------|------|
| `LogDebug` | JWT сохтан, роли мавчуд |
| `LogInformation` | Воридшавӣ, бакайдгирӣ, навсозӣ |
| `LogWarning` | Маълумоти нодуруст, корбар нест |
| `LogError` | Exception-хои гайричашмдошт |
