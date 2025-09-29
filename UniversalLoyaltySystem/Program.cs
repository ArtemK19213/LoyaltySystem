using LoyaltySystem.API.Infrastructure.Authorization;
using LoyaltySystem.API.Services;
using LoyaltySystem.API.Services.Implementations;
using LoyaltySystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure JWT settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// Register services
builder.Services.AddScoped<ILoyaltyAuthService, LoyaltyAuthService>();

// Configure Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();

// Set defaults if not configured
var key = string.IsNullOrEmpty(jwtSettings.Key)
    ? "DefaultSuperSecretKeyForLoyaltySystem256BitsLongEnoughForJWT"
    : jwtSettings.Key;

var issuer = string.IsNullOrEmpty(jwtSettings.Issuer)
    ? "LoyaltySystemAPI"
    : jwtSettings.Issuer;

var audience = string.IsNullOrEmpty(jwtSettings.Audience)
    ? "LoyaltySystemUsers"
    : jwtSettings.Audience;

// Validate key length
if (key.Length < 32)
{
    throw new ArgumentException("JWT Key must be at least 32 characters long for security");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ClockSkew = TimeSpan.Zero // Убираем задержку для точной проверки времени
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated successfully");
                return Task.CompletedTask;
            }
        };
    });

// Configure Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(LoyaltyRoles.Admin, policy =>
        policy.RequireRole(LoyaltyRoles.Admin));

    options.AddPolicy(LoyaltyRoles.Client, policy =>
        policy.RequireRole(LoyaltyRoles.Client));

    options.AddPolicy(LoyaltyRoles.Partner, policy =>
        policy.RequireRole(LoyaltyRoles.Partner));

    options.AddPolicy(LoyaltyRoles.Merchant, policy =>
        policy.RequireRole(LoyaltyRoles.Merchant));

    options.AddPolicy(LoyaltyPolicies.TierGold, policy =>
        policy.RequireClaim("Tier", "Gold", "Platinum"));

    options.AddPolicy(LoyaltyPolicies.CanRedeemPoints, policy =>
        policy.RequireRole(LoyaltyRoles.Client, LoyaltyRoles.Merchant)
              .RequireAssertion(context =>
                  context.User.HasClaim(c => c.Type == "Tier" &&
                  (c.Value == "Gold" || c.Value == "Platinum"))));
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // Log configured settings
    var configuredSettings = app.Services.GetRequiredService<IOptions<JwtSettings>>().Value;
    Console.WriteLine($"JWT Settings: Issuer={configuredSettings.Issuer}, Audience={configuredSettings.Audience}");
    Console.WriteLine($"Key length: {configuredSettings.Key?.Length ?? 0}");
}

// Serve static files from wwwroot
app.UseStaticFiles();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Маршруты для HTML страниц
app.MapGet("/login", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");
    if (File.Exists(filePath))
    {
        await context.Response.SendFileAsync(filePath);
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("Auth page not found. Create wwwroot/index.html file.");
    }
});

// Маршрут для профиля
app.MapGet("/profile", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "profile.html");
    if (File.Exists(filePath))
    {
        await context.Response.SendFileAsync(filePath);
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("Profile page not found. Create wwwroot/profile.html file.");
    }
});



// Health check endpoint
app.MapGet("/health", (IOptions<JwtSettings> jwtSettingsOptions) =>
{
    var jwtSettings = jwtSettingsOptions.Value;
    return Results.Ok(new
    {
        Message = "Loyalty System API is running!",
        Status = "Healthy",
        Timestamp = DateTime.UtcNow,
        JwtSettings = new
        {
            Issuer = jwtSettings.Issuer,
            Audience = jwtSettings.Audience,
            KeyLength = jwtSettings.Key?.Length ?? 0,
            AccessTokenExpiry = $"{jwtSettings.AccessTokenExpiry} minutes",
            RefreshTokenExpiry = $"{jwtSettings.RefreshTokenExpiry} minutes"
        }
    });
});

// Test endpoint to generate token
app.MapGet("/test-token", (IOptions<JwtSettings> jwtSettingsOptions) =>
{
    var jwtSettings = jwtSettingsOptions.Value;

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
    var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: jwtSettings.Issuer,
        audience: jwtSettings.Audience,
        claims: new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Email, "test@loyalty.com"),
            new Claim(ClaimTypes.MobilePhone, "+79998887766"),
            new Claim("Tier", "Gold"),
            new Claim("Points", "1000"),
            new Claim(ClaimTypes.Role, LoyaltyRoles.Client)
        },
        expires: DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpiry),
        signingCredentials: creds
    );

    return Results.Ok(new
    {
        Token = new JwtSecurityTokenHandler().WriteToken(token),
        ExpiresIn = jwtSettings.AccessTokenExpiry
    });
});


// Корневой маршрут - перенаправление на страницу логина
app.MapGet("/", async (HttpContext context) =>
{
    context.Response.Redirect("/login");
    await Task.CompletedTask;
});

// Маршрут для dashboard (после успешного входа)
app.MapGet("/dashboard", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(@"
        <!DOCTYPE html>
        <html lang='ru'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Dashboard - Система Лояльности</title>
            <style>
                body {
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                    min-height: 100vh;
                    margin: 0;
                    padding: 20px;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                }
                .dashboard-container {
                    background: white;
                    border-radius: 12px;
                    box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
                    padding: 30px;
                    max-width: 600px;
                    text-align: center;
                }
                h1 {
                    color: #4f46e5;
                    margin-bottom: 20px;
                }
                .btn {
                    padding: 12px 20px;
                    border: none;
                    border-radius: 8px;
                    font-size: 16px;
                    font-weight: 600;
                    cursor: pointer;
                    margin: 10px;
                    text-decoration: none;
                    display: inline-block;
                }
                .btn-primary {
                    background: #4f46e5;
                    color: white;
                }
                .btn-secondary {
                    background: #10b981;
                    color: white;
                }
                .btn-danger {
                    background: #ef4444;
                    color: white;
                }
            </style>
        </head>
        <body>
            <div class='dashboard-container'>
                <h1>Добро пожаловать в систему!</h1>
                <p>Вы успешно вошли в систему лояльности</p>
                <div>
                    <a href='/profile' class='btn btn-primary'>Мой профиль</a>
                    <a href='/login' class='btn btn-secondary'>Выйти</a>
                </div>
            </div>
        </body>
        </html>
    ");
});

app.Run();