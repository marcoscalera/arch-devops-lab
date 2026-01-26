using Microsoft.AspNetCore.Mvc.Testing;
using MyBank.Application; 
using MyBank.Api;         
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace MyBank.IntegrationTests;

public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiTests(WebApplicationFactory<Program> factory)
    {
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
        // arrange
        var request = new SaqueRequest(1, 100m);

        // act
        var response = await _client.PostAsJsonAsync("/saque", request);

        // assert
        response.EnsureSuccessStatusCode();
        
        var resultado = await response.Content.ReadFromJsonAsync<SaqueResponse>();
        
        Assert.Equal(900m, resultado?.NovoSaldo);
        Assert.Equal("Saque realizado com sucesso", resultado?.Mensagem);
    }
}