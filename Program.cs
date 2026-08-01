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
                            Type =
                                ReferenceType.SecurityScheme,
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

var allowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>()
    ?? Array.Empty<string>();

if (allowedOrigins.Length == 0)
{
    throw new InvalidOperationException(
        "At least one CORS origin must be configured.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "ReactApp",
        policy =>
        {
            policy
                .WithOrigins(allowedOrigins)
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

if (string.IsNullOrWhiteSpace(
    jwtSettings.Key))
{
    throw new InvalidOperationException(
        "JWT key was not configured.");
}

if (string.IsNullOrWhiteSpace(
    jwtSettings.Issuer))
{
    throw new InvalidOperationException(
        "JWT issuer was not configured.");
}

if (string.IsNullOrWhiteSpace(
    jwtSettings.Audience))
{
    throw new InvalidOperationException(
        "JWT audience was not configured.");
}

// --------------------------------------------------
// Database configuration
// --------------------------------------------------

var connectionString =
    builder.Configuration
        .GetConnectionString(
            "DefaultConnection");

if (string.IsNullOrWhiteSpace(
    connectionString))
{
    throw new InvalidOperationException(
        "The DefaultConnection connection string was not configured.");
}

// --------------------------------------------------
// Authentication
// --------------------------------------------------

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults
                .AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults
                .AuthenticationScheme;
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

                ValidateIssuerSigningKey =
                    true,

                ValidIssuer =
                    jwtSettings.Issuer,

                ValidAudience =
                    jwtSettings.Audience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtSettings.Key)),

                ClockSkew = TimeSpan.Zero,

                NameClaimType =
                    ClaimTypes.Name,

                RoleClaimType =
                    ClaimTypes.Role
            };
    });

// --------------------------------------------------
// Authorization
// --------------------------------------------------

builder.Services.AddAuthorization();

// --------------------------------------------------
// Output caching
// --------------------------------------------------

builder.Services.AddOutputCache(options =>
{
    options.AddPolicy(
        "ProductsCache",
        policy =>
        {
            policy
                .Expire(
                    TimeSpan.FromSeconds(30))
                .SetVaryByQuery(
                    "Search",
                    "Category",
                    "MinPrice",
                    "MaxPrice",
                    "SortBy",
                    "SortDirection",
                    "PageNumber",
                    "PageSize")
                .Tag("products");
        });
});

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
// Application dependencies
// --------------------------------------------------

builder.Services.AddSingleton<
    DbConnectionFactory>();

builder.Services.AddScoped<
    IProductRepository,
    ProductRepository>();

builder.Services.AddScoped<
    IOrderRepository,
    OrderRepository>();

builder.Services.AddScoped<
    IOrderService,
    OrderService>();

builder.Services.AddTransient<
    IDiscountCalculator,
    DiscountCalculator>();

builder.Services.AddScoped<
    IImageService,
    ImageService>();

builder.Services.AddScoped<
    IProductService,
    ProductService>();

// --------------------------------------------------
// Build application
// --------------------------------------------------

var app = builder.Build();

// --------------------------------------------------
// Middleware pipeline
// --------------------------------------------------

app.UseMiddleware<
    ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseCors("ReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

app.MapControllers();

app.Run();