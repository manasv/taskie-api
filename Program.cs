using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskieAPI.Data;
using TaskieAPI.Services;
using TaskieAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var env = builder.Environment;
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    if (env.IsProduction() && !string.IsNullOrEmpty(databaseUrl))
    {
        options.UseNpgsql(ConvertDatabaseUrl(databaseUrl));
    }
    else
    {
        options.UseSqlite("Data Source=taskie.db");
    }
});

var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET")
             ?? builder.Configuration["Jwt:Key"]
             ?? "OhNoYouGotMyKey";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://*:{port}");

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Auto-migrate DB
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();

static string ConvertDatabaseUrl(string url)
{
    var uri = new Uri(url);
    var userInfo = uri.UserInfo.Split(':', 2);
    var username = userInfo[0];
    var password = userInfo[1];
    var host = uri.Host;
    var port = uri.Port > 0 ? uri.Port : 5432;
    var db = uri.AbsolutePath.TrimStart('/');

    return $"Host={host};Port={port};Username={username};Password={password};Database={db};SSL Mode=Require;Trust Server Certificate=true;";
}
