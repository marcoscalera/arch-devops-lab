using Microsoft.AspNetCore.Mvc;
using MyBank.Application;
using MyBank.Domain;
using MyBank.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IContaRepository, ContaRepositoryMemoria>();
builder.Services.AddScoped<SaqueUseCase>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok("Healthy"));

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
});

app.Run();

namespace MyBank.Api
{
    public partial class Program { }
}