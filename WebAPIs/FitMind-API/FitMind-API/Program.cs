using FitMind_API.Data;
using FitMind_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FitMind_API.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("DeepAI", client =>
{
    var apiKey = builder.Configuration["DeepAI:ApiKey"];
    client.BaseAddress = new Uri("https://api.deepai.org/");
    client.DefaultRequestHeaders.Add("api-key", apiKey); // ✅ Lowercase "api-key"
});

builder.Services.AddScoped<DeepAiService>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });


    builder.Services.AddHttpClient("Sightengine", client =>
    {
        client.BaseAddress = new Uri("https://api.sightengine.com/1.0/");
    });
    builder.Services.AddScoped<SightengineService>();


// ✅ Services implementation
builder.Services.AddTransient<IEmailService, EmailService>();

// ✅ Database
builder.Services.AddDbContext<FMDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FMDBCS")));

var app = builder.Build();

// ✅ Use CORS before Auth
app.UseCors("AngularPolicy");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
        c.RoutePrefix = string.Empty;
    });
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
};

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
