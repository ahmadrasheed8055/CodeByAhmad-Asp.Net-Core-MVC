using FitMind_API.Data;
using FitMind_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FitMind_API.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger to use fully-qualified schema IDs to avoid collisions
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.FullName);
});

// ✅ CORS Policy for Angular only
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", builder =>
    {
        builder.WithOrigins("http://localhost:4200") // Replace with production Angular URL
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// ✅ JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            RoleClaimType = "role"
        };
    });

// ✅ Admin Authorization Policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireAssertion(context =>
        context.User.HasClaim(c => (c.Type == "role" || c.Type == System.Security.Claims.ClaimTypes.Role) && c.Value == "admin")
        || context.User.IsInRole("admin")));
});

builder.Services.AddHttpClient("Sightengine", client =>
{
    client.BaseAddress = new Uri("https://api.sightengine.com/1.0/");
});
builder.Services.AddScoped<SightengineService>();

// ✅ Chatbot Service
builder.Services.AddHttpClient<GeminiChatService>();
builder.Services.AddScoped<GeminiChatService>();

builder.Services.Configure<FitMind_API.Models.AdminSettings>(
    builder.Configuration.GetSection("AdminSettings"));

// ✅ Services implementation
builder.Services.AddTransient<IEmailService, EmailService>();

// ✅ Database
builder.Services.AddDbContext<FMDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FMDBCS")));

var app = builder.Build();

// Use CORS before Auth
app.UseCors("AngularPolicy");

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // show full exception details in dev

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed 10 verified trainers & standard categories
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<FMDBContext>();
        await TrainerSeeder.SeedTrainersAsync(dbContext);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Seeder Error]: {ex.Message}");
    }
}

app.Run();
