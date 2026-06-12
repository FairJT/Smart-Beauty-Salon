using System.Threading.RateLimiting;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartSalon.Data;
using SmartSalon.Models;
using SmartSalon.Services;
using SmartSalon.Validation;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── Identity ────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ─── JWT Authentication ──────────────────────────────────────
var jwtKey = builder.Configuration["JwtSettings:Key"]
    ?? Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? throw new InvalidOperationException(
        "JWT secret not found. Set JwtSettings:Key or JWT_SECRET env var.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        ValidateLifetime = true
    };
});

// ─── Authorization Policies ─────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireSuperAdmin", policy =>
        policy.RequireClaim("UserType", "SuperAdmin"));
    options.AddPolicy("RequireManager", policy =>
        policy.RequireClaim("UserType", "SuperAdmin", "SalonManager"));
    options.AddPolicy("RequireManagerOrAbove", policy =>
        policy.RequireClaim("UserType", "SuperAdmin", "SalonManager"));
    options.AddPolicy("RequireArtist", policy =>
        policy.RequireClaim("UserType", "SuperAdmin", "SalonManager", "Artist"));
    options.AddPolicy("RequireAuthenticated", policy =>
        policy.RequireAuthenticatedUser());
});

// ─── CORS ────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());

    options.AddPolicy("Production", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
            ?? new[] { "https://smartsalon.ir" };
        policy.WithOrigins(origins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ─── Rate Limiting ───────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 2;
    });

    options.AddFixedWindowLimiter("api", limiterOptions =>
    {
        limiterOptions.PermitLimit = 60;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
    });
});

// ─── Services ────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISalonService, SmartSalon.Services.SalonService>();
builder.Services.AddScoped<IArtistService, ArtistService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddHostedService<ReminderService>();

// ─── FluentValidation ────────────────────────────────────────
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ─── MVC + Swagger ───────────────────────────────────────────
builder.Services.AddRazorPages();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Bearer {token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
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
            message = "خطای داخلی سرور",
            detail = builder.Environment.IsDevelopment()
                ? context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error.Message
                : null
        });
    };
});

var app = builder.Build();

// ─── Auto-migrate database in Docker ─────────────────────────
if (app.Environment.EnvironmentName == "Docker")
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

// ─── Seed Roles ─────────────────────────────────────────────
using var roleScope = app.Services.CreateScope();
var roleManager = roleScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
string[] roles = { "SuperAdmin", "SalonManager", "Artist", "Client" };
foreach (var role in roles)
{
    if (!await roleManager.RoleExistsAsync(role))
        await roleManager.CreateAsync(new IdentityRole(role));
}

// ─── Middleware Pipeline ─────────────────────────────────────
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors(app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker" ? "AllowAll" : "Production");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.UseSession();
app.MapRazorPages();
app.MapControllers();
app.Run();
