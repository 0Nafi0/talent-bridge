using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TalentBridge.Api.Data;
using TalentBridge.Api.Models;
using TalentBridge.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Database ───────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Identity ───────────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ── JWT Authentication ─────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtSettings["Issuer"],
        ValidAudience            = jwtSettings["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew                = TimeSpan.Zero
    };

    // Reject tokens whose SecurityStamp has been rotated (e.g. after logout)
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async ctx =>
        {
            var userManager = ctx.HttpContext.RequestServices
                .GetRequiredService<UserManager<ApplicationUser>>();

            var userId = ctx.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                      ?? ctx.Principal?.FindFirst("sub")?.Value;

            var tokenStamp = ctx.Principal?.FindFirst("stampId")?.Value;

            if (userId is null || tokenStamp is null)
            {
                ctx.Fail("Missing claims.");
                return;
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user is null || user.SecurityStamp != tokenStamp)
            {
                ctx.Fail("Token has been invalidated.");
            }
        }
    };
});

// ── Authorization policies ─────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly",     policy => policy.RequireRole("Admin"));
    options.AddPolicy("RecruiterOnly", policy => policy.RequireRole("Recruiter", "Admin"));
    options.AddPolicy("CandidateOnly", policy => policy.RequireRole("Candidate"));
});

// ── Application services ───────────────────────────────────────────────
builder.Services.AddScoped<ITokenService, TokenService>();

// ── Controllers ────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Swagger with JWT support ───────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title   = "TalentBridge API",
        Version = "v1",
        Description = "Job Portal API — connecting candidates and recruiters."
    });

    // JWT Bearer definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter your JWT token here (without the 'Bearer' prefix)."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            []
        }
    });
});

var app = builder.Build();

// ── Auto-migrate + seed on startup ────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
}

// ── Middleware pipeline ────────────────────────────────────────────────
// Always enable Swagger (Great for portfolios & public testing)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TalentBridge API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
