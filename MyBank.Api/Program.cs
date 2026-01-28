using Microsoft.AspNetCore.Mvc;
using MyBank.Application;
using MyBank.Domain;
using MyBank.Infrastructure;
using Microsoft.OpenApi.Models; 
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "MyBank API", 
        Version = "v1",
        Description = "API de transações bancárias do MyBank"
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("BancoMyBank"));

builder.Services.AddScoped<IContaRepository, ContaRepositoryEF>();
builder.Services.AddScoped<SaqueUseCase>();

var app = builder.Build();

// carga inicial dos dados (seed)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated(); 
    
    if (!db.Contas.Any())
    {
        db.Contas.Add(new MyBank.Domain.Conta(1, 1000m, true)); 
        db.SaveChanges();
    }
}

// configuração do middleware (pipeline)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok("Healthy"))
   .WithOpenApi();

app.MapPost("/saque", ([FromBody] SaqueRequest req, [FromServices] SaqueUseCase useCase) => 
{
    try 
    {
        var response = useCase.Executar(req);
        return Results.Ok(response);
    }
    catch (ArgumentException ex) { return Results.BadRequest(ex.Message); }
    catch (InvalidOperationException ex) { return Results.BadRequest(ex.Message); }
    catch (Exception ex) { return Results.Problem(ex.Message); }
})
.WithName("RealizarSaque")
.WithOpenApi(x => new(x) // documenta quem usa
{
    Summary = "Realiza saque na conta",
    Description = "Esta rota é consumida pelo **App Mobile** e pelo **Front Angular**."
});

app.Run();

namespace MyBank.Api
{
    public partial class Program { }
}