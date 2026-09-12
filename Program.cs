using QualiTrack.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using QualiTrack.Filters;
using QualiTrack.Services;
using System.Text.Json.Serialization;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// ===============================================================================
// 1. DATABASE
// ===============================================================================
var connectionString = builder.Configuration.GetConnectionString("Supabase")
    ?? throw new InvalidOperationException("Connection string 'Supabase' not found.");

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(connectionString));

// ===============================================================================
// 2. JWT AUTH
// ===============================================================================
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"]!;
var jwtIssuer = jwtSection["Issuer"]!;
var jwtAudience = jwtSection["Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddScoped<QualiTrack.Services.IEmailService, QualiTrack.Services.EmailService>();
builder.Services.AddScoped<IQualityScoreService, QualityScoreService>();
builder.Services.AddScoped<IKpiService, KpiService>();
builder.Services.AddScoped<IRecentActivityService, RecentActivityService>();

var isRailway = Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT") != null;
var useS3 = isRailway || builder.Configuration["Storage:UseS3"] == "true";
if (useS3)
{
    builder.Services.AddScoped<IStorageService, S3StorageService>();
}
else
{
    builder.Services.AddScoped<IStorageService, LocalStorageService>();
}

builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 10 * 1024 * 1024);

builder.Services.AddScoped<ValidateUserFilter>();
builder.Services.AddScoped<PdfReportService>();

builder.Services.AddHttpClient();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidateUserFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter()
    );
    options.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var app = builder.Build();

// Seed data checklist template
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var pending = db.Database.GetPendingMigrations().ToArray();
    Console.WriteLine($"pending migrations count: {pending.Count()}");
    foreach (var m in pending)
        Console.WriteLine($"  - {m}");

    await db.Database.MigrateAsync();
    Console.WriteLine("MigrateAsync done");

    await DbSeeder.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseStaticFiles();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
