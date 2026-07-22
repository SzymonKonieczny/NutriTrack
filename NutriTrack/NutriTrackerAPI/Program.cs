using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NutriTrack.Application;
using NutriTrack.Application.Abstractions;
using NutriTrack.Domain.Data;
using NutriTrack.Identity;
using NutriTrack.Identity.Data;
using NutriTrackerAPI.Auth;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();

builder.Services.AddNutriTrackDomain(connectionString);
builder.Services.AddNutriTrackIdentity(builder.Configuration, connectionString);
builder.Services.AddNutriTrackApplication();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://www.nutritrack.pl", "https://api.nutritrack.pl")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            },
            Array.Empty<string>()
        },
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var identityDb = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    identityDb.Database.Migrate();
    var domainDb = scope.ServiceProvider.GetRequiredService<NutriTrackDbContext>();
    domainDb.Database.Migrate();

      // Seed roles
      var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
      foreach (var roleName in new[] { "Admin", "User" })
      {
          if (!await roleManager.RoleExistsAsync(roleName))
              await roleManager.CreateAsync(new IdentityRole(roleName));
      }
    
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
if (!app.Environment.IsDevelopment())
    app.Run("http://localhost:5100");
else 
    app.Run();