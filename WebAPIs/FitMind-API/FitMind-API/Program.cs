using FitMind_API.Data;
using FitMind_API.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Swagger (OpenAPI) support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//configure service
builder.Services.AddTransient<IEmailService, EmailService>();

//cors 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Configure DbContext with SQL Server connection string
builder.Services.AddDbContext<FMDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FMDBCS")));

var app = builder.Build();
app.UseCors("AllowAllOrigins");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Enable Swagger only in development mode
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
        c.RoutePrefix = string.Empty; // Swagger available at root URL
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
