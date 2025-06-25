using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer; // This import is needed for JWT
using Microsoft.IdentityModel.Tokens; // This import is needed for TokenValidationParameters
using System.Text; // This import is needed for Encoding.UTF8
using Api.Data;
using Api.Services; // Import our custom services

var builder = WebApplication.CreateBuilder(args);

// === SERVICE CONFIGURATION ===
// Register built-in services that ASP.NET Core needs

// Add controllers (handles HTTP requests and responses)
builder.Services.AddControllers();

// Add API documentation services (Swagger)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// === DATABASE CONFIGURATION ===
// Tell Entity Framework to use PostgreSQL with our connection string
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// === JWT AUTHENTICATION CONFIGURATION ===
// Configure JWT authentication tells .NET how to validate tokens
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Set up token validation parameters
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Validate that the token was issued by our app
            ValidateIssuer = true,
            
            // Validate that the token is intended for our app
            ValidateAudience = true,
            
            // Validate that the token hasn't expired
            ValidateLifetime = true,
            
            // Validate that the token signature is authentic
            ValidateIssuerSigningKey = true,
            
            // Set what values we expect for issuer and audience
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            
            // Set the key used to verify token signatures
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Add authorization services (handles [Authorize] attributes)
builder.Services.AddAuthorization();


//CORS สำหรับ Vue.js
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173", // Vite dev server
                "http://localhost:3000", // Vue CLI dev server  
                "http://127.0.0.1:5173", // Alternative localhost
                "http://127.0.0.1:3000"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // สำหรับ JWT tokens
    });
});
// === CUSTOM SERVICE REGISTRATION ===
// Register our custom services for dependency injection

// Register JWT service (creates and validates tokens)
builder.Services.AddScoped<IJwtService, JwtService>();

// Register password service (hashes and verifies passwords)
builder.Services.AddScoped<IPasswordService, PasswordService>();

// === CORS CONFIGURATION ===
// Configure Cross-Origin Resource Sharing (allows frontend apps to call our API)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()      // Allow requests from any domain
              .AllowAnyMethod()      // Allow any HTTP method (GET, POST, etc.)
              .AllowAnyHeader();     // Allow any headers
    });
});

// === BUILD THE APPLICATION ===
var app = builder.Build();

// === DATABASE INITIALIZATION ===
// Auto-migrate database when the app starts (creates tables if they don't exist)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();        // Apply any pending migrations
    context.Database.EnsureCreated();  // Ensure database exists
}

// === MIDDLEWARE PIPELINE CONFIGURATION ===
// Middleware runs in order for each request

// Enable Swagger in development (API documentation UI)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS (must come before authentication)
app.UseCors();

// Enable authentication (checks if user is logged in)
app.UseAuthentication();

// Enable authorization (checks if user has permission for specific endpoints)
app.UseAuthorization();

// Map controller endpoints (connects URLs to controller methods)
app.MapControllers();

// Start the application
app.Run();