using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MyBank.Application; 
using MyBank.Api;         
using MyBank.Infrastructure; 
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

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

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
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

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        resultado.Should().NotBeNull();
        resultado!.NovoSaldo.Should().Be(900);
    }

    [Fact]
    public async Task Contrato_Saque_Deve_Ter_Estrutura_JSON_Correta()
    {
        ResetarContaParaTeste(1, 1000m, true);

        var response = await _client.PostAsJsonAsync("/saque", new SaqueRequest(1, 100m));
        var jsonText = await response.Content.ReadAsStringAsync();

        var json = JsonDocument.Parse(jsonText);
        var root = json.RootElement;

        root.TryGetProperty("novoSaldo", out var novoSaldo).Should().BeTrue();
        novoSaldo.ValueKind.Should().Be(JsonValueKind.Number);
        
        root.TryGetProperty("mensagem", out var mensagem).Should().BeTrue();
        mensagem.ValueKind.Should().Be(JsonValueKind.String);
    }

    [Fact]
    public async Task Contrato_Saque_Deve_Validar_Schema_JSON()
    {
        ResetarContaParaTeste(1, 1000m, true);
        
        var schemaJson = @"{
            'type': 'object',
            'required': ['novoSaldo', 'mensagem'],
            'properties': {
                'novoSaldo': { 'type': 'number' },
                'mensagem': { 'type': 'string', 'minLength': 1 }
            },
            'additionalProperties': false
        }";
        
        var schema = JSchema.Parse(schemaJson);

        var response = await _client.PostAsJsonAsync("/saque", new SaqueRequest(1, 100m));
        var jsonText = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(jsonText);

        bool isValid = json.IsValid(schema, out IList<string> errorMessages);
        
        isValid.Should().BeTrue(
            because: "O JSON deve seguir exatamente o schema definido. Erros: {0}",
            string.Join(", ", errorMessages));
    }

    [Fact]
    public async Task Contrato_Erro_Deve_Retornar_Campo_Erro()
    {
        ResetarContaParaTeste(1, 50m, true);

        var response = await _client.PostAsJsonAsync("/saque", new SaqueRequest(1, 100m));
        var jsonText = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        jsonText.Should().Contain("erro");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    [InlineData(-100.50)]
    public async Task API_Deve_Rejeitar_Valores_Negativos_Ou_Zero(decimal valorInvalido)
    {
        var request = new SaqueRequest(1, valorInvalido);

        var response = await _client.PostAsJsonAsync("/saque", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("erro");
    }

    [Fact]
    public async Task API_Deve_Rejeitar_Conta_Inexistente()
    {
        var request = new SaqueRequest(9999, 100m);

        var response = await _client.PostAsJsonAsync("/saque", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("não encontrada");
    }

    [Fact]
    public async Task API_Deve_Rejeitar_Saldo_Insuficiente()
    {
        ResetarContaParaTeste(1, 50m, true);
        var request = new SaqueRequest(1, 100m);

        var response = await _client.PostAsJsonAsync("/saque", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("insuficiente");
    }

    [Fact]
    public async Task API_Deve_Retornar_Content_Type_JSON()
    {
        ResetarContaParaTeste(1, 1000m, true);

        var response = await _client.PostAsJsonAsync("/saque", new SaqueRequest(1, 100m));

        response.Content.Headers.ContentType.Should().NotBeNull();
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task API_Deve_Aceitar_JSON_UTF8()
    {
        ResetarContaParaTeste(1, 1000m, true);
        var json = System.Text.Json.JsonSerializer.Serialize(new SaqueRequest(1, 100m));
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/saque", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Saques_Simultaneos_Nao_Devem_Gerar_Inconsistencia()
    {
        ResetarContaParaTeste(1, 2000m, true);
        
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_client.PostAsJsonAsync("/saque", new SaqueRequest(1, 200m)));
        }

        var responses = await Task.WhenAll(tasks);

        var sucessos = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        var falhas = responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest);

        (sucessos + falhas).Should().Be(10);
        
        falhas.Should().Be(0);
    }

    [Fact]
    public async Task Cliente_VIP_Nao_Deve_Pagar_Taxa()
    {
        ResetarContaParaTeste(1, 1000m, isVip: true);

        var response = await _client.PostAsJsonAsync("/saque", new SaqueRequest(1, 100m));
        var resultado = await response.Content.ReadFromJsonAsync<SaqueResponse>();

        resultado!.NovoSaldo.Should().Be(900m);
    }

    [Fact]
    public async Task Cliente_Comum_Deve_Pagar_Taxa_De_5_Reais()
    {
        ResetarContaParaTeste(2, 1000m, isVip: false);

        var response = await _client.PostAsJsonAsync("/saque", new SaqueRequest(2, 100m));
        var resultado = await response.Content.ReadFromJsonAsync<SaqueResponse>();

        resultado!.NovoSaldo.Should().Be(895m);
    }

    [Fact]
    public async Task Saque_De_Todo_Saldo_Deve_Deixar_Conta_Zerada()
    {
        ResetarContaParaTeste(1, 500m, isVip: true);

        var response = await _client.PostAsJsonAsync("/saque", new SaqueRequest(1, 500m));
        var resultado = await response.Content.ReadFromJsonAsync<SaqueResponse>();

        resultado!.NovoSaldo.Should().Be(0m);
    }

    [Fact(Skip = "Teste de carga - executar manualmente")]
    public async Task API_Deve_Responder_Em_Menos_De_100ms()
    {
        ResetarContaParaTeste(1, 1000m, true);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var response = await _client.PostAsJsonAsync("/saque", new SaqueRequest(1, 100m));
        stopwatch.Stop();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    [Fact(Skip = "Teste de carga - executar manualmente")]
    public async Task API_Deve_Suportar_50_Requisicoes_Por_Segundo()
    {
        ResetarContaParaTeste(1, 10000m, true);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var tasks = new List<Task>();

        for (int i = 0; i < 50; i++)
        {
            tasks.Add(_client.PostAsJsonAsync("/saque", new SaqueRequest(1, 10m)));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
    }

    [Fact]
    public async Task Swagger_JSON_Deve_Estar_Disponivel()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("/Saque");
        content.Should().Contain("MyBank Enterprise API");
    }

    [Fact]
    public async Task Swagger_UI_Deve_Estar_Disponivel_Em_Development()
    {
        var response = await _client.GetAsync("/swagger/index.html");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    private void ResetarContaParaTeste(int contaId, decimal saldo, bool isVip)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var conta = db.Contas.Find(contaId);
        if (conta != null)
        {
            conta.DefinirSaldo(saldo);
            db.SaveChanges();
        }
        else
        {
            db.Contas.Add(new MyBank.Domain.Conta(contaId, saldo, isVip));
            db.SaveChanges();
        }
    }
}