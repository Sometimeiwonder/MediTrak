using MediTrack.Mvc.Data;
using MediTrack.Mvc.Models;
using MediTrack.Mvc.Options;
using MediTrack.Mvc.Repositories;
using MediTrack.Mvc.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/lab05-.txt", rollingInterval: RollingInterval.Day));

builder.Services.AddControllers(options =>
{
    options.Filters.Add<MediTrack.Mvc.Filters.AuditAccessDeniedFilter>();
});

builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LogoutPath = "/Account/Logout";
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewSupply", p => p.RequireRole("Admin", "Staff"));
    options.AddPolicy("CanManageSupply", p => p.RequireRole("Admin"));
    options.AddPolicy("CanAdjustStock", p => p.RequireRole("Admin", "Staff"));
    options.AddPolicy("CanViewAuditLog", p => p.RequireRole("Admin"));
    options.AddPolicy("CanManageIssue", p => p.RequireRole("Admin", "Staff"));
});

builder.Services.AddScoped<ISupplyRepository, SupplyRepository>();
builder.Services.AddScoped<ISupplyService, SupplyService>();
builder.Services.AddScoped<IIssueRepository, IssueRepository>();
builder.Services.AddScoped<IIssueService, IssueService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Application is running."), tags: new[] { "live" })
    .AddDbContextCheck<AppDbContext>("database", tags: new[] { "ready" });

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] =
            context.HttpContext.TraceIdentifier;
        context.ProblemDetails.Extensions["timestamp"] =
            DateTimeOffset.UtcNow;
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SPA fallback - serve index.html for all non-API routes
app.MapFallbackToFile("index.html");

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };
        await context.Response.WriteAsJsonAsync(result);
    }
});

app.MapGet("/api/supplies/{id:int}", async (int id, AppDbContext db, HttpContext http) =>
{
    var supply = await db.MediTrack
        .AsNoTracking()
        .FirstOrDefaultAsync(s => s.Id == id);

    if (supply == null)
    {
        return Results.Problem(
            type: "https://example.com/problems/supply-not-found",
            title: "Supply not found",
            detail: $"The supply with id {id} was not found.",
            statusCode: StatusCodes.Status404NotFound,
            instance: http.Request.Path);
    }

    return Results.Ok(supply);
});

app.MapGet("/api/supplies/search", async (string? keyword, int? minPrice, int? maxPrice, AppDbContext db, HttpContext http) =>
{
    if (string.IsNullOrWhiteSpace(keyword))
    {
        return Results.ValidationProblem(
            errors: new Dictionary<string, string[]> { { "keyword", new[] { "Keyword is required and cannot be empty." } } },
            title: "Validation Error",
            type: "https://example.com/problems/validation",
            detail: "The keyword parameter must not be empty.",
            instance: http.Request.Path);
    }

    if (keyword.Length > 100)
    {
        return Results.ValidationProblem(
            errors: new Dictionary<string, string[]> { { "keyword", new[] { "Keyword must not exceed 100 characters." } } },
            title: "Validation Error",
            type: "https://example.com/problems/validation",
            detail: "The keyword parameter is too long.",
            instance: http.Request.Path);
    }

    var query = db.MediTrack
        .AsNoTracking()
        .AsQueryable();

    query = query.Where(s =>
        s.Name.Contains(keyword) ||
        s.Code.Contains(keyword) ||
        (s.Description != null && s.Description.Contains(keyword)));

    if (minPrice.HasValue)
        query = query.Where(s => s.UnitPrice >= minPrice.Value);
    if (maxPrice.HasValue)
        query = query.Where(s => s.UnitPrice <= maxPrice.Value);

    var results = await query.ToListAsync();

    if (results.Count == 0)
    {
        return Results.Problem(
            type: "https://example.com/problems/no-results",
            title: "No supplies found",
            detail: $"No supplies found matching keyword '{keyword}'.",
            statusCode: StatusCodes.Status404NotFound,
            instance: http.Request.Path);
    }

    return Results.Ok(results);
});

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    await DbInitializer.SeedIdentityAsync(services);
}

app.Run();
