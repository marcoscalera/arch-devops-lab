using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MyBank.Application;
using MyBank.Domain;
using MyBank.Infrastructure;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("BancoMyBank"));

builder.Services.AddScoped<IContaRepository, ContaRepositoryEF>();
builder.Services.AddScoped<SaqueUseCase>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "MyBank Enterprise API", 
        Version = "v1",
        Description = "API migrada para arquitetura de Controllers."
    });

    // pega o texto escrito no Controller e joga no Swagger
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();

// seed do banco - dados inicias
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    if (!db.Contas.Any())
    {
        db.Contas.Add(new Conta(1, 1000m, true));
        db.SaveChanges();
    }
}

// pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers(); 
app.MapGet("/health", () => Results.Ok("Healthy")); 

app.Run();

namespace MyBank.Api { public partial class Program { } }