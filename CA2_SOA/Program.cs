using System.Text;
using System.Text.Json.Serialization;
using CA2SOA.Auth;
using CA2SOA.Data;
using CA2SOA.Entities;
using CA2SOA.Repositories;
using CA2SOA.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CA2_SOA;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddControllers()
            .AddJsonOptions(o =>
            {
                // Allows enums to be sent as strings
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "CA2_SOA", Version = "v1" });

            // Bearer auth in Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Paste the JWT token value (starts with 'eyJ'). Do not include 'Bearer '.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            c.OperationFilter<SwaggerAuthOperationFilter>();
        });

        builder.Services.AddDbContext<GameShelfDbContext>(opt =>
        {
            var cs = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(cs)) cs = "Data Source=gameshelf.db";
            opt.UseSqlite(cs);
        });

        builder.Services.AddScoped<IGameService, GameService>();
        builder.Services.AddScoped<IGenreService, GenreService>();
        builder.Services.AddScoped<ILibraryEntryService, LibraryEntryService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IReviewService, ReviewService>();

        // Auth (JWT)
        builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
        builder.Services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

        var jwtKey = builder.Configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey))
            throw new InvalidOperationException("Missing Jwt:Key in appsettings.json (use 16+ characters).");

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                var issuer = builder.Configuration["Jwt:Issuer"] ?? "CA2SOA";
                var audience = builder.Configuration["Jwt:Audience"] ?? "CA2SOA";

                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();
        });

        var app = builder.Build();

        // DB create + seed genres
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GameShelfDbContext>();
            db.Database.EnsureCreated();

            if (!db.Genres.Any())
            {
                db.Genres.AddRange(
                    new Genre { Name = "Action" },
                    new Genre { Name = "RPG" },
                    new Genre { Name = "Platformer" },
                    new Genre { Name = "Shooter" },
                    new Genre { Name = "Puzzle" }
                );
                db.SaveChanges();
            }
        }

        app.UseHttpsRedirection();

        var iconPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "gameicon.png");

        app.Use(async (ctx, next) =>
        {
            var p = ctx.Request.Path.Value;

            if ((p == "/swagger/favicon-16x16.png" || p == "/swagger/favicon-32x32.png") && File.Exists(iconPath))
            {
                ctx.Response.ContentType = "image/png";
                await ctx.Response.SendFileAsync(iconPath);
                return;
            }

            await next();
        });

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "CA2SOA v1");
            c.ConfigObject.PersistAuthorization = true;
        });

        // Website at client (wwwroot/client/index.html)
        var clientRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "client");

        app.UseDefaultFiles(new DefaultFilesOptions
        {
            RequestPath = "/client",
            FileProvider = new PhysicalFileProvider(clientRoot),
            DefaultFileNames = { "index.html" }
        });

        app.UseStaticFiles(new StaticFileOptions
        {
            RequestPath = "/client",
            FileProvider = new PhysicalFileProvider(clientRoot)
        });

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MapGet("/", () => Results.Redirect("/client")).AllowAnonymous();

        app.Run();
    }
}

sealed class SwaggerAuthOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var allowAnonymous =
            context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() ||
            (context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() ?? false);

        if (allowAnonymous) return;

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    }
}
