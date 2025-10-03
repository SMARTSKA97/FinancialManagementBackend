using FinancialPlanner.API.Extensions;
using FinancialPlanner.Application;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.Categories;
using FinancialPlanner.Application.Services;
using FinancialPlanner.Infrastructure.Persistence;
using FinancialPlanner.Infrastructure.Repositories;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// --- Refactored Dependency Injection --
builder.Services.AddPresentationLayerServices(builder.Configuration);

var app = builder.Build();
const string CorsPolicy = "AllowSpecificOrigins";
builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, p =>
    p.WithOrigins("https://financialplannerfrontend.karmakarsubhrajit680.workers.dev")
    .AllowAnyHeader()
    .AllowAnyMethod()
    //.AllowCredentials()
));
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

// --- App Configuration ---
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        db.Database.Migrate(); // runs EF migrations automatically
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine("MIGRATION FAILED: " + ex.Message);
        throw; // fail fast so you see it in Render logs
    }
}

app.Run();