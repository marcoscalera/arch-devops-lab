using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MyBank.Application; 
using MyBank.Api;         
using MyBank.Infrastructure; 
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace MyBank.IntegrationTests;

public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory; 

    public ApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory; 
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_Deve_Retornar_200_OK()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Saque_VIP_Deve_Funcionar_E2E()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            var conta = db.Contas.Find(1);
            if (conta != null)
            {
                conta.DefinirSaldo(1000m);
                db.SaveChanges();
            }
        }

        var request = new SaqueRequest(1, 100m);

        var response = await _client.PostAsJsonAsync("/saque", request);
        var resultado = await response.Content.ReadFromJsonAsync<SaqueResponse>();

        response.EnsureSuccessStatusCode();
        
        Assert.NotNull(resultado); 
        Assert.Equal(900, resultado.NovoSaldo);
    }

    [Fact]
    public async Task Contrato_Saque_Deve_Manter_Nomes_Das_Propriedades()
    {
        var request = new SaqueRequest(1, 100m);

        var response = await _client.PostAsJsonAsync("/saque", request);
        
        // leitura crua do json retornado
        var jsonText = await response.Content.ReadAsStringAsync();

        // se renomear "NovoSaldo" para "Saldo" falha
        Assert.Contains("novoSaldo", jsonText); 
        Assert.Contains("mensagem", jsonText);
    }
}