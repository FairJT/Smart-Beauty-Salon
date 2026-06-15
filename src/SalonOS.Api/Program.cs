using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SalonOS.Api.Authorization;
using SalonOS.Api.Middleware;
using SalonOS.Shared.Authorization;
using SalonOS.Infrastructure.Identity;
using SalonOS.Infrastructure.Admin;
using SalonOS.Infrastructure.Interceptors;
using SalonOS.Infrastructure.MultiTenancy;
using SalonOS.Shared.Identity;
using SalonOS.Identity.API.Middleware;
using SalonOS.Identity.Domain;
using SalonOS.Identity.Domain.Enums;
using SalonOS.Identity.Infrastructure;
using SalonOS.Infrastructure;
using SalonOS.Infrastructure.Jobs;
using SalonOS.Booking.Infrastructure;
using SalonOS.Catalog.Infrastructure;
using SalonOS.Shared;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();

// Add EF Core for Identity
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add EF Core for main app
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.AddInterceptors(sp.GetRequiredService<TenantSessionContextInterceptor>());
});

// Add EF Core for Booking module
builder.Services.AddDbContext<BookingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add EF Core for Catalog module
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add ASP.NET Identity
builder.Services.AddIdentity<ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<IdentityDbContext>();

// Add Hangfire
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

builder.Services.AddHangfireServer();

// ─── Event handlers ──────────────────────────────────────────
builder.Services.AddScoped<SalonOS.Infrastructure.EventHandlers.BookingCompletedHandler>();
builder.Services.AddScoped<SalonOS.Infrastructure.EventHandlers.BookingCancelledHandler>();
builder.Services.AddScoped<SalonOS.Infrastructure.EventHandlers.InventoryLowHandler>();

// Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtKey = builder.Configuration["JwtSettings:Key"];
    if (string.IsNullOrEmpty(jwtKey))
        jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET");
    if (string.IsNullOrEmpty(jwtKey))
        throw new InvalidOperationException("JWT key is not configured.");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// IHttpContextAccessor + ICurrentUser (Task 3.1)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// Add Authorization — permission-based policies (§R6.1)
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddScoped<IAuthorizationHandler, OwnsAppointmentHandler>(); // Task 5.1

// Add Tenant Context (scoped per request)
// TenantContext now reads from ICurrentUser claims — never from request input (R3, R4)
builder.Services.AddScoped<ITenantContext, TenantContextFromClaims>();

// RLS session-context interceptor (Task 8.2)
builder.Services.AddScoped<TenantSessionContextInterceptor>();

// PlatformAdminService — the only sanctioned cross-tenant service (Task 7.1)
builder.Services.AddScoped<PlatformAdminService>();

// Add Identity Services
builder.Services.AddScoped<IAuthService, AuthService>();

// Add Booking Services
builder.Services.AddScoped<IBookingService, BookingService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    options.AddPolicy("Production", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                      ?? ["https://smartsalon.ir"];
        policy.WithOrigins(origins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ─── Rate Limiting ─────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 2;
    });

    options.AddFixedWindowLimiter("api", limiterOptions =>
    {
        limiterOptions.PermitLimit = 60;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
    });
});

// ─── Global Exception Handler ────────────────────────────────
builder.Services.AddExceptionHandler(options =>
{
    options.ExceptionHandler = context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsJsonAsync(new
        {
            message = "Internal server error",
            detail = builder.Environment.IsDevelopment()
                ? context.Features.Get<IExceptionHandlerFeature>()?.Error.Message
                : null
        });
    };
});

var app = builder.Build();

    // ─── Auto-migrate & seed database in Docker ─────────────────
    if (app.Environment.EnvironmentName == "Docker")
    {
        using var scope = app.Services.CreateScope();
        var identityDb = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var appDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var bookingDb = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
        var catalogDb = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Apply pending migrations if any
        try { identityDb.Database.Migrate(); } catch { }
        try { appDb.Database.Migrate(); } catch { }

        // Databases are created by Migrate() above.
        // Raw SQL below provides idempotent schema safety for environments
        // without generated migrations (FAIR-02).

        // Ensure OutboxMessages table exists (shared DB with IdentityDbContext)
        appDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OutboxMessages')
            CREATE TABLE [OutboxMessages] (
                [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
                [EventType] NVARCHAR(200) NOT NULL,
                [Payload] NVARCHAR(MAX) NOT NULL,
                [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                [ProcessedAt] DATETIME2 NULL,
                [Error] NVARCHAR(MAX) NULL,
                [RetryCount] INT NOT NULL DEFAULT 0
            )");

        // Apply Booking migrations if any
        try { bookingDb.Database.Migrate(); } catch { }

        // ── Ensure Phase 2 booking tables exist ──
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ArtistSchedules')
            CREATE TABLE [ArtistSchedules] (
                [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
                [TenantId] UNIQUEIDENTIFIER NOT NULL,
                [ArtistId] UNIQUEIDENTIFIER NOT NULL,
                [DayOfWeek] INT NOT NULL,
                [StartTime] TIME NOT NULL,
                [EndTime] TIME NOT NULL,
                [IsActive] BIT NOT NULL DEFAULT 1,
                [IsDeleted] BIT NOT NULL DEFAULT 0,
                [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                [UpdatedAt] DATETIME2 NULL,
                [DeletedAt] DATETIME2 NULL
            )");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Leaves')
            CREATE TABLE [Leaves] (
                [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
                [TenantId] UNIQUEIDENTIFIER NOT NULL,
                [ArtistId] UNIQUEIDENTIFIER NOT NULL,
                [StartDateTime] DATETIME2 NOT NULL,
                [EndDateTime] DATETIME2 NOT NULL,
                [Reason] NVARCHAR(500) NULL,
                [Status] INT NOT NULL DEFAULT 1,
                [IsDeleted] BIT NOT NULL DEFAULT 0,
                [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                [UpdatedAt] DATETIME2 NULL,
                [DeletedAt] DATETIME2 NULL
            )");

        // ── Ensure profile tables exist (for DBs created before profiles were added) ──
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SalonManagerProfiles')
            CREATE TABLE [SalonManagerProfiles] (
                [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
                [UserId] NVARCHAR(450) NOT NULL,
                [TenantId] UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
                [SalonId] INT NULL,
                [IsOwner] BIT NOT NULL DEFAULT 0,
                [IsActive] BIT NOT NULL DEFAULT 1,
                [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                CONSTRAINT [FK_SalonManagerProfiles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
            )");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SalonManagerProfiles_UserId')
            CREATE UNIQUE INDEX [IX_SalonManagerProfiles_UserId] ON [SalonManagerProfiles]([UserId])");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ArtistProfiles')
            CREATE TABLE [ArtistProfiles] (
                [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
                [UserId] NVARCHAR(450) NOT NULL,
                [TenantId] UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
                [SalonId] INT NULL,
                [Skill] NVARCHAR(100) NOT NULL DEFAULT '',
                [Bio] NVARCHAR(MAX) NULL,
                [IsActive] BIT NOT NULL DEFAULT 1,
                [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                CONSTRAINT [FK_ArtistProfiles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
            )");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ArtistProfiles_UserId')
            CREATE UNIQUE INDEX [IX_ArtistProfiles_UserId] ON [ArtistProfiles]([UserId])");

        // ── Ensure SavedSalon table exists ──
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SavedSalons')
            CREATE TABLE [SavedSalons] (
                [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
                [UserId] NVARCHAR(450) NOT NULL,
                [Slug] NVARCHAR(450) NOT NULL DEFAULT '',
                [SalonName] NVARCHAR(200) NOT NULL DEFAULT '',
                [LogoUrl] NVARCHAR(500) NULL,
                [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
            )");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SavedSalons_UserId')
            CREATE INDEX [IX_SavedSalons_UserId] ON [SavedSalons]([UserId])");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SavedSalons_UserId_Slug')
            CREATE UNIQUE INDEX [IX_SavedSalons_UserId_Slug] ON [SavedSalons]([UserId], [Slug])");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ClientProfiles')
            CREATE TABLE [ClientProfiles] (
                [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
                [UserId] NVARCHAR(450) NOT NULL,
                [LoyaltyPoints] INT NOT NULL DEFAULT 0,
                [TotalVisits] INT NOT NULL DEFAULT 0,
                [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                CONSTRAINT [FK_ClientProfiles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
            )");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClientProfiles_UserId')
            CREATE UNIQUE INDEX [IX_ClientProfiles_UserId] ON [ClientProfiles]([UserId])");

        // ── Ensure JobSeekerProfiles table exists ──
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'JobSeekerProfiles')
            CREATE TABLE [JobSeekerProfiles] (
                [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
                [UserId] NVARCHAR(450) NOT NULL,
                [Resume] NVARCHAR(MAX) NOT NULL DEFAULT '',
                [WorkHistory] NVARCHAR(MAX) NOT NULL DEFAULT '',
                [Skills] NVARCHAR(MAX) NOT NULL DEFAULT '',
                [Location] NVARCHAR(200) NOT NULL DEFAULT '',
                [PreferredRole] NVARCHAR(100) NULL,
                [ExpectedSalary] INT NOT NULL DEFAULT 0,
                [IsActive] BIT NOT NULL DEFAULT 1,
                [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                CONSTRAINT [FK_JobSeekerProfiles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
            )");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_JobSeekerProfiles_UserId')
            CREATE UNIQUE INDEX [IX_JobSeekerProfiles_UserId] ON [JobSeekerProfiles]([UserId])");

        // ── Ensure Phase 1 catalog tables exist ──
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ServiceTypes')
            CREATE TABLE [ServiceTypes] (
                [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
                [Name] NVARCHAR(200) NOT NULL,
                [Category] NVARCHAR(100) NOT NULL DEFAULT '',
                [Description] NVARCHAR(MAX) NULL,
                [IsActive] BIT NOT NULL DEFAULT 1,
                [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
            )");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SalonMedia')
            CREATE TABLE [SalonMedia] (
                [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
                [TenantId] UNIQUEIDENTIFIER NOT NULL,
                [SalonId] UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
                [Url] NVARCHAR(500) NOT NULL,
                [MediaType] NVARCHAR(20) NOT NULL DEFAULT 'image',
                [SortOrder] INT NOT NULL DEFAULT 0,
                [AltText] NVARCHAR(200) NULL,
                [IsActive] BIT NOT NULL DEFAULT 1,
                [IsDeleted] BIT NOT NULL DEFAULT 0,
                [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                [UpdatedAt] DATETIME2 NULL,
                [DeletedAt] DATETIME2 NULL
            )");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CatalogServices')
            CREATE TABLE [CatalogServices] (
                [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
                [Name] NVARCHAR(200) NOT NULL,
                [Description] NVARCHAR(MAX) NULL,
                [ServiceTypeId] UNIQUEIDENTIFIER NOT NULL,
                [BasePrice_Amount] BIGINT NOT NULL DEFAULT 0,
                [BasePrice_Currency] NVARCHAR(3) NOT NULL DEFAULT 'IRR',
                [BaseDurationMinutes] INT NOT NULL DEFAULT 30,
                [TenantId] UNIQUEIDENTIFIER NOT NULL,
                [IsActive] BIT NOT NULL DEFAULT 1,
                [IsDeleted] BIT NOT NULL DEFAULT 0,
                [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                [UpdatedAt] DATETIME2 NULL,
                [DeletedAt] DATETIME2 NULL
            )");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ServiceOptions')
            CREATE TABLE [ServiceOptions] (
                [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
                [CatalogServiceId] UNIQUEIDENTIFIER NOT NULL,
                [Name] NVARCHAR(200) NOT NULL,
                [Description] NVARCHAR(MAX) NULL,
                [PriceDelta_Amount] BIGINT NOT NULL DEFAULT 0,
                [PriceDelta_Currency] NVARCHAR(3) NOT NULL DEFAULT 'IRR',
                [DurationDeltaMinutes] INT NOT NULL DEFAULT 0,
                [TenantId] UNIQUEIDENTIFIER NOT NULL,
                [IsActive] BIT NOT NULL DEFAULT 1,
                [IsDeleted] BIT NOT NULL DEFAULT 0,
                [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                [UpdatedAt] DATETIME2 NULL,
                [DeletedAt] DATETIME2 NULL
            )");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Materials')
            CREATE TABLE [Materials] (
                [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
                [Name] NVARCHAR(200) NOT NULL,
                [Description] NVARCHAR(MAX) NULL,
                [Price_Amount] BIGINT NOT NULL DEFAULT 0,
                [Price_Currency] NVARCHAR(3) NOT NULL DEFAULT 'IRR',
                [Unit] NVARCHAR(50) NULL,
                [TenantId] UNIQUEIDENTIFIER NOT NULL,
                [IsActive] BIT NOT NULL DEFAULT 1,
                [IsDeleted] BIT NOT NULL DEFAULT 0,
                [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                [UpdatedAt] DATETIME2 NULL,
                [DeletedAt] DATETIME2 NULL
            )");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CatalogServiceMaterials')
            CREATE TABLE [CatalogServiceMaterials] (
                [CatalogServiceId] UNIQUEIDENTIFIER NOT NULL,
                [MaterialId] UNIQUEIDENTIFIER NOT NULL,
                PRIMARY KEY ([CatalogServiceId], [MaterialId])
            )");
        // Update Identity tables with Phase 1 columns (IF NOT EXISTS for each)
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tenants') AND name = 'PrimaryColor')
                ALTER TABLE [Tenants] ADD [PrimaryColor] NVARCHAR(20) NULL");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tenants') AND name = 'FontColor')
                ALTER TABLE [Tenants] ADD [FontColor] NVARCHAR(20) NULL");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tenants') AND name = 'License')
                ALTER TABLE [Tenants] ADD [License] NVARCHAR(100) NULL");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tenants') AND name = 'Grade')
                ALTER TABLE [Tenants] ADD [Grade] NVARCHAR(50) NULL");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tenants') AND name = 'Fax')
                ALTER TABLE [Tenants] ADD [Fax] NVARCHAR(30) NULL");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tenants') AND name = 'Address')
                ALTER TABLE [Tenants] ADD [Address] NVARCHAR(500) NULL");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tenants') AND name = 'Phone')
                ALTER TABLE [Tenants] ADD [Phone] NVARCHAR(30) NULL");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tenants') AND name = 'Email')
                ALTER TABLE [Tenants] ADD [Email] NVARCHAR(200) NULL");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tenants') AND name = 'WorkingHours')
                ALTER TABLE [Tenants] ADD [WorkingHours] NVARCHAR(500) NULL");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ArtistProfiles') AND name = 'ContractType')
                ALTER TABLE [ArtistProfiles] ADD [ContractType] INT NOT NULL DEFAULT 1");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ArtistProfiles') AND name = 'SalaryAmount')
                ALTER TABLE [ArtistProfiles] ADD [SalaryAmount] BIGINT NULL");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ArtistProfiles') AND name = 'SalaryCurrency')
                ALTER TABLE [ArtistProfiles] ADD [SalaryCurrency] NVARCHAR(3) NULL");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ArtistProfiles') AND name = 'RentAmount')
                ALTER TABLE [ArtistProfiles] ADD [RentAmount] BIGINT NULL");
        identityDb.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ArtistProfiles') AND name = 'RentCurrency')
                ALTER TABLE [ArtistProfiles] ADD [RentCurrency] NVARCHAR(3) NULL");
        // Apply Catalog migrations if any
        try { catalogDb.Database.Migrate(); } catch { }

        // ── Seed users ──────────────────────────────────────────
        await SeedUsersAsync(userManager, identityDb);
    }

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseRateLimiter();
app.UseHttpsRedirection();

app.UseCors(app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker" ? "AllowAll" : "Production");

app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>(); // Task 4.5 — reject missing-tenant requests
app.UseAuthorization();

// Add Tenant Context Middleware
app.UseMiddleware<TenantContextMiddleware>();

app.MapControllers();

// ─── Hangfire recurring jobs ─────────────────────────────────
app.UseHangfireDashboard();
RecurringJob.AddOrUpdate<OutboxDispatcherJob>(
    "outbox-dispatcher",
    j => j.ExecuteAsync(CancellationToken.None),
    "*/1 * * * *"); // every minute
RecurringJob.AddOrUpdate<ReminderJob>(
    "appointment-reminders",
    j => j.ExecuteAsync(CancellationToken.None),
    "*/5 * * * *"); // every 5 minutes

app.Run();

// ─── Seed helper ─────────────────────────────────────────────────
static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, IdentityDbContext db)
{
    var seedUsers = new[]
    {
        new { Mobile = "09110000001", Password = "Test@1234", FirstName = "مدیر", LastName = "سامانه", Type = UserType.SuperAdmin },
        new { Mobile = "09110000002", Password = "Test@1234", FirstName = "مدیر", LastName = "سالن", Type = UserType.SalonManager },
        new { Mobile = "09110000003", Password = "Test@1234", FirstName = "هنرمند", LastName = "نمونه", Type = UserType.Artist },
        new { Mobile = "09110000004", Password = "Test@1234", FirstName = "مشتری", LastName = "نمونه", Type = UserType.Client },
    };

    foreach (var u in seedUsers)
    {
        if (await userManager.FindByNameAsync(u.Mobile) != null)
            continue;

        var user = new ApplicationUser
        {
            UserName = u.Mobile,
            PhoneNumber = u.Mobile,
            FirstName = u.FirstName,
            LastName = u.LastName,
            NationalCode = "1234567890",
            UserType = u.Type
        };

        var result = await userManager.CreateAsync(user, u.Password);
        if (!result.Succeeded)
        {
            Console.WriteLine($"[Seed] Failed to create {u.Type}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            continue;
        }

        Console.WriteLine($"[Seed] Created {u.Type}: {u.Mobile} / {u.Password}");

    }

    // Create a default tenant FIRST so profile FK references are valid
    var defaultTenantId = Guid.NewGuid();
    var salonManagerUser = await userManager.FindByNameAsync("09110000002");
    if (salonManagerUser != null && !await db.Tenants.AnyAsync())
    {
        var tenant = new Tenant
        {
            Id = defaultTenantId,
            Name = "سالن زیبایی نمونه",
            Slug = "salon-sample",
            Description = "سالن زیبایی پیشفرض",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();
        Console.WriteLine("[Seed] Created default tenant");
    }
    else
    {
        var existing = await db.Tenants.FirstOrDefaultAsync();
        if (existing != null)
            defaultTenantId = existing.Id;
    }

    // Batch-create role-specific profile entities for newly seeded users
    var profileBatch = new List<object>();
    foreach (var u in seedUsers)
    {
        var user = await userManager.FindByNameAsync(u.Mobile);
        if (user == null) continue;

        switch (u.Type)
        {
            case UserType.SalonManager:
                if (!await db.SalonManagerProfiles.AnyAsync(p => p.UserId == user.Id))
                    profileBatch.Add(new SalonManagerProfile { UserId = user.Id, TenantId = defaultTenantId, IsOwner = false, IsActive = true });
                break;
            case UserType.Artist:
                if (!await db.ArtistProfiles.AnyAsync(p => p.UserId == user.Id))
                    profileBatch.Add(new ArtistProfile { UserId = user.Id, TenantId = defaultTenantId, Skill = "General", IsActive = true });
                break;
            case UserType.Client:
                if (!await db.ClientProfiles.AnyAsync(p => p.UserId == user.Id))
                    profileBatch.Add(new ClientProfile { UserId = user.Id });
                break;
        }
    }
    if (profileBatch.Count > 0)
    {
        db.AddRange(profileBatch);
        await db.SaveChangesAsync();
        Console.WriteLine($"[Seed] Created {profileBatch.Count} profile(s)");
    }

    // Create memberships for non-SuperAdmin users
    if (salonManagerUser != null && !await db.Memberships.AnyAsync())
    {
        var tenant = await db.Tenants.FirstAsync();

        var memberships = new[]
        {
            new Membership { UserId = salonManagerUser.Id, TenantId = tenant.Id, Role = MembershipRole.Manager, IsActive = true },
        };

        var artist = await userManager.FindByNameAsync("09110000003");
        if (artist != null)
            memberships = [.. memberships, new Membership { UserId = artist.Id, TenantId = tenant.Id, Role = MembershipRole.Staff, IsActive = true }];

        var client = await userManager.FindByNameAsync("09110000004");
        if (client != null)
            memberships = [.. memberships, new Membership { UserId = client.Id, TenantId = tenant.Id, Role = MembershipRole.Member, IsActive = true }];

        db.Memberships.AddRange(memberships);
        await db.SaveChangesAsync();
        Console.WriteLine("[Seed] Created default tenant memberships");
    }
}
