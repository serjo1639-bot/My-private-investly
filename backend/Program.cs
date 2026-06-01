using System.Text.Json;
using Investly.API.Data;
using Investly.API.Infrastructure.Extensions;
using Investly.API.Infrastructure.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ─── Services ────────────────────────────────────────────────────────────────

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAppServices();
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddControllers().AddJsonOptions(o =>
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt();

// ─── App ─────────────────────────────────────────────────────────────────────

var app = builder.Build();

// Auto-migrate and seed on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await DbSeeder.SeedAsync(db);
}

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Investly API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseStaticFiles();
app.UseCors("AllowFrontends");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
