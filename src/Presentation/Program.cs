using API.Extensions;
using API.Middleware;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.CookiePolicy;

var builder = WebApplication.CreateBuilder(args);

// --- Dependency Injection Setup ---
// This handles all service registrations (Application, Infrastructure, API)
builder.Services.AddPresentationLayerServices(builder.Configuration);

// Add Memory Cache for Custom Rate Limiting
builder.Services.AddMemoryCache();

// Configure for Render.com reverse proxy (Cloudflare CDN + Render proxy)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    // Trust all proxies - required for cloud platforms like Render.com
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
    options.ForwardLimit = 2; // Limit to 2 proxy hops (Cloudflare + Render)
});

var app = builder.Build();

// --- Middleware Pipeline ---
// 1. Forwarded Headers (Must be first to identify client IP)
app.UseForwardedHeaders();

// 2. Global Exception Handler
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 3. Health Checks (Liveness/Readiness)
app.MapGet("/healthz", () => Results.Ok("ok"));
app.MapGet("/health/db", async (ApplicationDbContext db) =>
{
    return await db.Database.CanConnectAsync()
        ? Results.Ok("db-ok")
        : Results.Problem("db-fail", statusCode: 503);
});

// CSRF Token Endpoint (for Angular to retrieve anti-forgery token)
app.MapGet("/api/antiforgery/token", (HttpContext context) =>
{
    var antiforgery = context.RequestServices.GetRequiredService<Microsoft.AspNetCore.Antiforgery.IAntiforgery>();
    var tokens = antiforgery.GetAndStoreTokens(context);
    context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, new CookieOptions 
    { 
        HttpOnly = false, // Angular needs to read this
        Secure = true,
        SameSite = SameSiteMode.Strict
    });
    return Results.Ok(new { token = tokens.RequestToken });
}).RequireCors("AllowSpecificOrigins");

// 4. Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.Always
});

app.UseRateLimiter();

// 5. CORS
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        // db.Database.Migrate(); // Disabled for least-privilege user support
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}
app.UseCors("AllowSpecificOrigins");

// Security Headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    // Tightened CSP: Removed 'unsafe-inline' from script-src (if possible, otherwise keep minimal), added object-src 'none', base-uri 'self'
    // Note: If Angular requires inline styles/scripts, we might need 'unsafe-inline' or hashes.
    // For now, we will try to be strict but keep 'unsafe-inline' for styles as Angular often needs it for component styles.
    // We removed 'unsafe-inline' from script-src to prevent XSS.
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; connect-src 'self' https:; object-src 'none'; base-uri 'self';");
    await next();
});

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<SessionValidationMiddleware>();
app.UseMiddleware<UpdateLastSeenMiddleware>();

app.MapControllers();

app.Run();