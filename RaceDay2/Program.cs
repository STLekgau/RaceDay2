
using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RaceDay2.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT access token."
    });

    options.AddSecurityRequirement(document =>
       new OpenApiSecurityRequirement
       {
           [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
       });
});

// Database services
builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<EventRepository>();
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<EventCategoryRepository>();
builder.Services.AddScoped<EnrollmentRepository>();
builder.Services.AddScoped<ResultRepository>();
builder.Services.AddScoped<DatabaseConnectionTest>();

// JWT configuration
var jwtKey = builder.Configuration["Jwt:Key"];
var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey) ||
    string.IsNullOrWhiteSpace(issuer) ||
    string.IsNullOrWhiteSpace(audience))
{
    throw new InvalidOperationException(
        "JWT Key, Issuer and Audience must be configured in User Secrets.");
}

byte[] keyBytes;

try
{
    keyBytes = Convert.FromBase64String(jwtKey);
}
catch (FormatException)
{
    throw new InvalidOperationException(
        "Jwt:Key must be a valid Base64-encoded secret.");
}

if (keyBytes.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key must contain at least 32 decoded bytes.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,

            ValidateAudience = true,
            ValidAudience = audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Keep this order
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
