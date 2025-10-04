using FinancialPlanner.API.Extensions;
using FinancialPlanner.Application;
using FinancialPlanner.Infrastructure.Persistence;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- Add all services BEFORE Build() ---
builder.Services.AddPresentationLayerServices(builder.Configuration);

// 🔹 Register CORS policy here (before Build)
const string CorsPolicy = "AllowSpecificOrigins";
builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, p =>
    p.WithOrigins(
        "https://financialplannerfrontend.karmakarsubhrajit680.workers.dev",
        "http://localhost:4200"
    )
    .AllowAnyHeader()
    .AllowAnyMethod()
// .AllowCredentials() // only if you need cookies/auth
));

var app = builder.Build();

// --- Middleware / app config ---
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
});

app.MapGet("/healthz", () => Results.Ok("ok"));
app.MapGet("/health/db", async (ApplicationDbContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();
    return canConnect
        ? Results.Ok("db-ok")
        : Results.Problem("db-fail", statusCode: 503);
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// 🔹 Apply the CORS policy after building
app.UseCors(CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// --- Run EF migrations at startup ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try { db.Database.Migrate(); }
    catch (Exception ex)
    {
        Console.Error.WriteLine("MIGRATION FAILED: " + ex);
        throw;
    }
}

app.Run();
