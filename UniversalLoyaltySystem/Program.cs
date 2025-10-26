using System.Security.Claims;
using System.Text;
using LoyaltySystem.API.Data;
using LoyaltySystem.API.Infrastructure;
using LoyaltySystem.API.Infrastructure.Middleware;
using LoyaltySystem.API.Models.Entities;
using LoyaltySystem.API.Services.Implementations;
using LoyaltySystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

var cs = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(cs));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
        opts.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                if (string.IsNullOrEmpty(ctx.Token) && ctx.Request.Cookies.TryGetValue("access_token", out var cookie))
                    ctx.Token = cookie;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("AdminOnly",   p => p.RequireRole(Roles.Admin));
    o.AddPolicy("PartnerOnly", p => p.RequireRole(Roles.Partner));
    o.AddPolicy("ClientOnly",  p => p.RequireRole(Roles.Client));
});

builder.Services.AddScoped<ILoyaltyAuthService, LoyaltyAuthService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ILoyaltyService, LoyaltyService>();
var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Гейт для /html/profile.html
app.Use(async (ctx, next) =>
{
    var path = ctx.Request.Path.Value ?? string.Empty;
    if (path.Equals("/html/profile.html", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/html/partner/", StringComparison.OrdinalIgnoreCase))
    {
        var authed = ctx.User?.Identity?.IsAuthenticated ?? false;
        var isPartner = authed && (ctx.User.IsInRole("Partner") || ctx.User.IsInRole("Admin"));
        if (!authed) { ctx.Response.Redirect("/html/login.html"); return; }
        if (path.StartsWith("/html/partner/", StringComparison.OrdinalIgnoreCase) && !isPartner)
        { ctx.Response.Redirect("/html/login.html"); return; }
    }
    await next();
});




app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

// Seed admin
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    if (!db.Users.Any(u => u.Role == Roles.Admin))
    {
        db.Users.Add(new LoyaltyUser
        {
            Email = "admin@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Passw0rd!"),
            FullName = "Platform Admin",
            Role = Roles.Admin
        });
        db.SaveChanges();
    }
}



app.MapGet("/", () => Results.Redirect("/html/register.html"));



app.Run();
