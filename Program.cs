using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ShoppingApp.Api.Configuration;
using ShoppingApp.Api.Interfaces;
using ShoppingApp.Api.Middleware;
using ShoppingApp.Api.Models;
using ShoppingApp.Api.Repositories;
using ShoppingApp.Api.Services;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// Controllers
// --------------------------------------------------

builder.Services.AddControllers();

// --------------------------------------------------
// Swagger
// --------------------------------------------------

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Shopping App API",
            Version = "v1"
        });

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Enter the JWT token. Swagger adds the Bearer prefix automatically."
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});

// --------------------------------------------------
// CORS
// --------------------------------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "ReactApp",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// --------------------------------------------------
// JWT settings
// --------------------------------------------------

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(
        JwtSettings.SectionName));

var jwtSettings =
    builder.Configuration
        .GetSection(JwtSettings.SectionName)
        .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "JWT settings were not found.");

if (string.IsNullOrWhiteSpace(jwtSettings.Key))
{
    throw new InvalidOperationException(
        "JWT key was not configured.");
}

// --------------------------------------------------
// Authentication
// --------------------------------------------------

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtSettings.Key)),
                ClockSkew = TimeSpan.Zero,
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };
    });

// --------------------------------------------------
// Authorization
// --------------------------------------------------

builder.Services.AddAuthorization();

// --------------------------------------------------
// Authentication dependencies
// --------------------------------------------------

builder.Services.AddScoped<
    IUserRepository,
    UserRepository>();

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddScoped<
    IJwtTokenService,
    JwtTokenService>();

builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>>();

// --------------------------------------------------
// Existing registrations
// --------------------------------------------------

builder.Services.AddSingleton<DbConnectionFactory>();

builder.Services.AddScoped<
    IProductRepository,
    ProductRepository>();

builder.Services.AddScoped<
    IOrderRepository,
    OrderRepository>();

builder.Services.AddScoped<
    IOrderService,
    OrderService>();

builder.Services.AddScoped<
    IProductService,
    ProductService>();

var app = builder.Build();

// --------------------------------------------------
// Middleware pipeline
// --------------------------------------------------

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("ReactApp");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
