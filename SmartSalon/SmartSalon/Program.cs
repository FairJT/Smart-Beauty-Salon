using System.Text;
using FirebaseAdmin;
using FluentValidation;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartSalon.Data;
using SmartSalon.Models;
using SmartSalon.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Database ────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Identity ────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 4;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ── JWT Authentication ─────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtKey = jwtSettings["Key"] ?? "DefaultSecretKey_ChangeMe!@#$%^&*()_+";

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
    };
});

// ── Authorization Policies ──────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireSuperAdmin", policy =>
        policy.RequireRole("SuperAdmin"));
});

// ── CORS ────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:8081", "http://127.0.0.1:8081", "http://flutter-web")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ── Controllers + Validation ────────────────────────────────────
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Session ─────────────────────────────────────────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
});

// ── Firebase ────────────────────────────────────────────────────
try
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile("firebase-credentials.json"),
    });
}
catch
{
    // Firebase is optional — skip if credentials file is missing
}

// ── Application Services ────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IArtistService, ArtistService>();
builder.Services.AddScoped<ISalonService, SmartSalon.Services.SalonService>();
builder.Services.AddScoped<IServiceService, SmartSalon.Services.ServiceService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

// ── Background Services ─────────────────────────────────────────
builder.Services.AddHostedService<ReminderService>();

var app = builder.Build();

// ── Auto-migrate database ───────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!app.Environment.IsEnvironment("Testing"))
    {
        db.Database.Migrate();
    }
}

// ── Middleware Pipeline ─────────────────────────────────────────
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
