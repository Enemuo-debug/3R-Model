using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NiRAProject.Data;
using NiRAProject.models;
using Microsoft.AspNetCore.Identity;
using NiRAProject.Tools;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ReplaceThisWithYourSecretKey_AtLeast32Chars";
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

// ---------------------------
// Services
// ---------------------------
builder.Services.AddControllers();

// Swagger with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Add JWT auth to swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. " +
                      "Enter 'Bearer {token}' or use the AuthToken cookie. Example: 'Bearer eyJhbGci...'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// DB Context
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentityCore<RRRModel>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddSignInManager<SignInManager<RRRModel>>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<KeyOperations>();
builder.Services.AddScoped<TokenHelper>();
builder.Services.AddScoped<SendEmails>();

// IMPORTANT: Add Authentication and set default schemes (JWT Bearer)
builder.Services.AddAuthentication(options =>
{
    // Set the default authentication & challenge scheme to JwtBearer
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Configure token validation
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };

    // Optional: allow the token to be read from a cookie named "AuthToken" as well
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // 1) Try Authorization header: "Bearer <token>"
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Token = authHeader.Substring("Bearer ".Length).Trim();
                return Task.CompletedTask;
            }

            // 2) Fallback to cookie "AuthToken" (useful for swagger or browser calls)
            var cookieToken = context.Request.Cookies["AuthToken"];
            if (!string.IsNullOrEmpty(cookieToken))
            {
                context.Token = cookieToken;
            }

            return Task.CompletedTask;
        }
    };
});

// Add Authorization (policies can be added here)
builder.Services.AddAuthorization();

var app = builder.Build();

// ---------------------------
// HTTP request pipeline
// ---------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Important: Authentication must come before Authorization and before endpoints that require auth
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();