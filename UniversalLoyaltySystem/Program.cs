using LoyaltySystem.API.Infrastructure.Authorization;
using LoyaltySystem.API.Services;
using LoyaltySystem.API.Services.Implementations;
using LoyaltySystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger (если нужен Ч пока отключим)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// JWT settings from config
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// DI
builder.Services.AddScoped<ILoyaltyAuthService, LoyaltyAuthService>();

// Read and validate JWT settings
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
var key = string.IsNullOrWhiteSpace(jwt.Key) ? "DefaultSuperSecretKeyForLoyaltySystem256BitsLongEnoughForJWT" : jwt.Key;
var issuer = string.IsNullOrWhiteSpace(jwt.Issuer) ? "LoyaltySystemAPI" : jwt.Issuer;
var audience = string.IsNullOrWhiteSpace(jwt.Audience) ? "LoyaltySystemUsers" : jwt.Audience;

if (key.Length < 32)
    throw new ArgumentException("JWT Key must be at least 32 characters long");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(LoyaltyRoles.Admin, p => p.RequireRole(LoyaltyRoles.Admin));
    options.AddPolicy(LoyaltyRoles.Client, p => p.RequireRole(LoyaltyRoles.Client));
    options.AddPolicy(LoyaltyRoles.Partner, p => p.RequireRole(LoyaltyRoles.Partner));
    options.AddPolicy(LoyaltyRoles.Merchant, p => p.RequireRole(LoyaltyRoles.Merchant));

    options.AddPolicy(LoyaltyPolicies.TierGold, p => p.RequireClaim("Tier", "Gold", "Platinum"));
    options.AddPolicy(LoyaltyPolicies.CanRedeemPoints, p =>
        p.RequireRole(LoyaltyRoles.Client, LoyaltyRoles.Merchant)
         .RequireAssertion(ctx => ctx.User.HasClaim(c => c.Type == "Tier" && (c.Value == "Gold" || c.Value == "Platinum"))));
});

builder.Services.AddCors(o => o.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    var configured = app.Services.GetRequiredService<IOptions<JwtSettings>>().Value;
    Console.WriteLine($"JWT: Issuer={configured.Issuer}, Audience={configured.Audience}, KeyLen={configured.Key?.Length ?? 0}");
}

app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// HTML demo routes
app.MapGet("/", (HttpContext ctx) =>
{
    ctx.Response.Redirect("/login");
    return Task.CompletedTask;
});

app.MapGet("/login", async (HttpContext ctx) =>
{
    ctx.Response.ContentType = "text/html";
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");
    if (File.Exists(filePath)) await ctx.Response.SendFileAsync(filePath);
    else { ctx.Response.StatusCode = 404; await ctx.Response.WriteAsync("Create wwwroot/index.html"); }
});

app.MapGet("/profile", async (HttpContext ctx) =>
{
    ctx.Response.ContentType = "text/html";
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "profile.html");
    if (File.Exists(filePath)) await ctx.Response.SendFileAsync(filePath);
    else { ctx.Response.StatusCode = 404; await ctx.Response.WriteAsync("Create wwwroot/profile.html"); }
});

app.MapGet("/health", (IOptions<JwtSettings> opt) =>
{
    var s = opt.Value;
    return Results.Ok(new
    {
        Message = "OK",
        Jwt = new { s.Issuer, s.Audience, KeyLength = s.Key?.Length ?? 0, s.AccessTokenExpiry, s.RefreshTokenExpiry }
    });
});

// quick smoke-token
app.MapGet("/test-token", (IOptions<JwtSettings> opt) =>
{
    var s = opt.Value;
    var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(s.Key)), SecurityAlgorithms.HmacSha256);
    var claims = new List<Claim>
    {
        // ЅџЋќ:
        // new(ClaimTypes.NameIdentifier, "test-user-id"),

        // —“јЋќ: фикс Ч реальный GUID
        new(ClaimTypes.NameIdentifier, "ebfcd8e2-6900-464c-b7eb-d6832fad53fb"),
        new(ClaimTypes.Email, "test@loyalty.com"),
        new(ClaimTypes.Role, LoyaltyRoles.Client),
        new("Tier", "Gold"),
        new("Points", "1000")
    };
    var token = new JwtSecurityToken(s.Issuer, s.Audience, claims,
        expires: DateTime.UtcNow.AddMinutes(s.AccessTokenExpiry),
        signingCredentials: creds);
    return Results.Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token), ExpiresIn = s.AccessTokenExpiry });
});


app.MapGet("/cards", async (HttpContext ctx) =>
{
    ctx.Response.ContentType = "text/html";
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cards.html");
    if (File.Exists(filePath)) await ctx.Response.SendFileAsync(filePath);
    else { ctx.Response.StatusCode = 404; await ctx.Response.WriteAsync("Create wwwroot/cards.html"); }
});


app.Run();
