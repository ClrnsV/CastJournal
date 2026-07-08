using CastJournal.Application;
using CastJournal.Domain.Entities;
using CastJournal.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());// Serialize enums as strings for it to be more readable in the API responses
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token below (no need to type 'Bearer ' prefix)."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
// Add AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(AssemblyReference).Assembly);
});

// Add Infrastructure (includes Identity + JWT settings)
builder.Services.AddInfrastructure(builder.Configuration);

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(jwtSettings["Secret"]!))
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var jti = context.Principal?.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti);
            if (jti == null)
            {
                context.Fail("Token missing jti claim.");
                return;
            }

            var revokedTokenRepository = context.HttpContext.RequestServices
                .GetRequiredService<CastJournal.Application.Interfaces.Repositories.IRevokedTokenRepository>();

            if (await revokedTokenRepository.IsRevokedAsync(jti))
            {
                context.Fail("This token has been revoked. Please log in again.");
                return;
            }

            var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            var userManager = context.HttpContext.RequestServices
                .GetRequiredService<UserManager<User>>();
            var user = userId == null ? null : await userManager.FindByIdAsync(userId);

            if (user == null || !user.IsActive)
            {
                context.Fail("This account has been deactivated.");
            }
        }
    };
});

builder.Services.AddAuthorization();

// CORS
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

// Seeding runs in every environment — production needs roles, an admin account,
// and reference data (species, content categories) just as much as local dev does.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CastJournal.Infrastructure.Persistence.Context.ApplicationDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<CastJournal.Domain.Entities.User>>();

    // Applies any pending EF Core migrations automatically on startup —
    // needed since we won't have local Package Manager Console access to the hosted DB.
    await context.Database.MigrateAsync();

    await CastJournal.Infrastructure.Persistence.Seed.ApplicationDbSeeder.SeedEssentialDataAsync(context, roleManager, userManager, app.Configuration);

    if (app.Environment.IsDevelopment())
    {
        await CastJournal.Infrastructure.Persistence.Seed.ApplicationDbSeeder.SeedDevelopmentDataAsync(context, userManager);
    }
}

// Configure pipeline
// Configure pipeline
var enableSwagger = app.Configuration.GetValue<bool>("SwaggerSettings:EnableSwagger", false);

if (app.Environment.IsDevelopment() || enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Auto redirect to Swagger
    app.Use(async (context, next) =>
    {
        if (context.Request.Path == "/")
        {
            context.Response.Redirect("/swagger");
            return;
        }
        await next();
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseMiddleware<CastJournal.Api.Middleware.ExceptionHandlingMiddleware>();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();


app.Run();