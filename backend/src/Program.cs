using UniversityManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UniversityManagement.Infrastructure.Auth;
using UniversityManagement.API.Middleware;
using UniversityManagement.API.Filters;
using UniversityManagement.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt");

builder.Services.AddDbContext<UniversityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add memory cache
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024; // Limit cache size to 1024 entries
});

// Register services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<TeacherService>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<EnrollmentService>();
builder.Services.AddScoped<GradeService>();
builder.Services.AddScoped<SortingService>();
builder.Services.AddScoped<DashboardService>();

// Register cache service
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<UserProfileService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Add token settings
builder.Services.Configure<TokenSettingsDto>(builder.Configuration.GetSection("TokenSettings"));

// Add file settings
builder.Services.Configure<FileSettingsDto>(builder.Configuration.GetSection("FileSettings"));

// Register cached services (decorator pattern)
builder.Services.AddScoped<IDashboardService>(provider =>
{
    var baseService = new DashboardService(
        provider.GetRequiredService<UniversityDbContext>(),
        provider.GetRequiredService<ILogger<DashboardService>>()
    );
    return new CachedDashboardService(
        provider.GetRequiredService<UniversityDbContext>(),
        provider.GetRequiredService<ILogger<CachedDashboardService>>(),
        provider.GetRequiredService<ICacheService>(),
        baseService
    );
});

builder.Services.AddScoped<IStudentService>(provider =>
{
    var baseService = new StudentService(
        provider.GetRequiredService<UniversityDbContext>()
    );
    return new CachedStudentService(
        provider.GetRequiredService<UniversityDbContext>(),
        provider.GetRequiredService<ILogger<CachedStudentService>>(),
        provider.GetRequiredService<ICacheService>(),
        baseService
    );
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]))
        };
    });

builder.Services.AddAuthorization();

// Add global exception filter
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
    options.Filters.Add<EnhancedValidationFilter>();
    options.Filters.Add<ModelValidationFilter>();
}).AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "University Management API",
        Version = "v1",
        Description = "A comprehensive university management system with authentication, authorization, caching, and advanced features."
    });
});

var app = builder.Build();

// Add error handling middleware (should be first)
app.UseMiddleware<ErrorHandlingMiddleware>();

// Add validation middleware
app.UseMiddleware<ValidationMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "University Management API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Warm up cache on startup
using (var scope = app.Services.CreateScope())
{
    var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Starting cache warm-up...");
        await cacheService.WarmUpCacheAsync();
        logger.LogInformation("Cache warm-up completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Cache warm-up failed");
    }
}

app.Run();
