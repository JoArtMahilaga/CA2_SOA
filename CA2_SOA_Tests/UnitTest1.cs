using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CA2SOA.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace CA2_SOA_Tests;

public sealed class ApiFactory : WebApplicationFactory<CA2_SOA.Program>
{
    private readonly SqliteConnection _conn = new("Data Source=:memory:;Cache=Shared");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _conn.Open();
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            const string Key64 =
                "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF";

            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = Key64,
                ["Jwt:Issuer"] = "CA2SOA",
                ["Jwt:Audience"] = "CA2SOA",

                ["JwtOptions:Key"] = Key64,
                ["JwtOptions:Issuer"] = "CA2SOA",
                ["JwtOptions:Audience"] = "CA2SOA",
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<GameShelfDbContext>));
            services.AddDbContext<GameShelfDbContext>(opt => opt.UseSqlite(_conn));

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GameShelfDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _conn.Dispose();
    }
}

public sealed class UnitTest1 : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public UnitTest1(ApiFactory factory) => _factory = factory;

    private HttpClient CreateClient()
        => _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

    private static async Task<string> ReadBody(HttpResponseMessage res)
        => await res.Content.ReadAsStringAsync();

    private static int? TryGetIdFromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;

        using var doc = JsonDocument.Parse(json);

       
        if (doc.RootElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var p in doc.RootElement.EnumerateObject())
            {
                if (string.Equals(p.Name, "id", StringComparison.OrdinalIgnoreCase) &&
                    p.Value.ValueKind == JsonValueKind.Number &&
                    p.Value.TryGetInt32(out var id))
                    return id;
            }
        }

        return null;
    }

    private static async Task<int> FindGameIdByTitleAsync(HttpClient client, string title)
    {
        var res = await client.GetAsync("/api/games");
        res.EnsureSuccessStatusCode();

        var json = await ReadBody(res);
        if (string.IsNullOrWhiteSpace(json)) return 0;

        using var doc = JsonDocument.Parse(json);

        if (doc.RootElement.ValueKind != JsonValueKind.Array) return 0;

        foreach (var el in doc.RootElement.EnumerateArray())
        {
            if (el.ValueKind != JsonValueKind.Object) continue;

            string? t = null;
            int id = 0;

            foreach (var p in el.EnumerateObject())
            {
                if (string.Equals(p.Name, "title", StringComparison.OrdinalIgnoreCase) &&
                    p.Value.ValueKind == JsonValueKind.String)
                    t = p.Value.GetString();

                if (string.Equals(p.Name, "id", StringComparison.OrdinalIgnoreCase) &&
                    p.Value.ValueKind == JsonValueKind.Number)
                    p.Value.TryGetInt32(out id);
            }

            if (!string.IsNullOrWhiteSpace(t) &&
                string.Equals(t, title, StringComparison.OrdinalIgnoreCase) &&
                id > 0)
                return id;
        }

        return 0;
    }

    private static async Task<int> GetAnyGenreIdAsync(HttpClient client)
    {
        var res = await client.GetAsync("/api/genres");
        res.EnsureSuccessStatusCode();

        var json = await ReadBody(res);
        if (string.IsNullOrWhiteSpace(json)) return 1;

        using var doc = JsonDocument.Parse(json);

        if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
        {
            var first = doc.RootElement[0];
            if (first.ValueKind == JsonValueKind.Object)
            {
                foreach (var p in first.EnumerateObject())
                {
                    if (string.Equals(p.Name, "id", StringComparison.OrdinalIgnoreCase) &&
                        p.Value.ValueKind == JsonValueKind.Number &&
                        p.Value.TryGetInt32(out var id) &&
                        id > 0)
                        return id;
                }
            }
        }

        return 1;
    }

    private static async Task<string> ExtractTokenOrThrow(HttpResponseMessage res)
    {
        var body = await ReadBody(res);
        using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);

        if (doc.RootElement.ValueKind != JsonValueKind.Object)
            throw new Exception("Auth response was not a JSON object:\n" + body);

        foreach (var p in doc.RootElement.EnumerateObject())
        {
            if (string.Equals(p.Name, "token", StringComparison.OrdinalIgnoreCase) &&
                p.Value.ValueKind == JsonValueKind.String)
            {
                var token = p.Value.GetString();
                if (!string.IsNullOrWhiteSpace(token)) return token!;
            }
        }

        throw new Exception("No token returned:\n" + body);
    }

    private async Task<string> RegisterAndLoginAsync(HttpClient client)
    {
        var user = "u" + Guid.NewGuid().ToString("N")[..10];
        var email = $"{user}@example.com";
        var pass = "Password123!";

        var reg = await client.PostAsJsonAsync("/api/auth/register", new
        {
            userName = user,
            email,
            password = pass
        });

        Assert.True(
            reg.StatusCode is HttpStatusCode.OK or HttpStatusCode.Created,
            $"Register expected 200/201, got {(int)reg.StatusCode} {reg.StatusCode}\n{await ReadBody(reg)}"
        );

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            userNameOrEmail = user,
            password = pass
        });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var token = await ExtractTokenOrThrow(login);
        Assert.StartsWith("eyJ", token, StringComparison.OrdinalIgnoreCase);
        return token;
    }

    private async Task<HttpClient> CreateAuthedClientAsync()
    {
        var client = CreateClient();
        var token = await RegisterAndLoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }


    [Fact]
    public async Task SwaggerJson_IsReachable()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var body = await res.Content.ReadAsStringAsync();
        Assert.Contains("openapi", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SwaggerUi_Index_IsReachable()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/swagger/index.html");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task Client_Index_Is_Public()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/client/index.html");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task Client_Css_Is_Public()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/client/styles.css");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task Client_AppJs_Is_Public()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/client/app.js");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task Client_ThemeJs_Is_Public()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/client/theme.js");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }
    

    [Fact]
    public async Task Games_GET_RequireAuth()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/api/games");

        Assert.True(
            res.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden,
            $"Expected 401/403, got {(int)res.StatusCode} {res.StatusCode}"
        );
    }

    [Fact]
    public async Task Games_POST_RequireAuth()
    {
        var client = CreateClient();
        var res = await client.PostAsJsonAsync("/api/games", new { });

        Assert.True(
            res.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden,
            $"Expected 401/403, got {(int)res.StatusCode} {res.StatusCode}"
        );
    }

    [Fact]
    public async Task Genres_RequireAuth()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/api/genres");

        Assert.True(
            res.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden,
            $"Expected 401/403, got {(int)res.StatusCode} {res.StatusCode}"
        );
    }

    [Fact]
    public async Task Auth_Me_RequiresAuth()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/api/auth/me");

        Assert.True(
            res.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden,
            $"Expected 401/403, got {(int)res.StatusCode} {res.StatusCode}"
        );
    }
    
    }
