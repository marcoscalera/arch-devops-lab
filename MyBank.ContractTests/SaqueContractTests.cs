using PactNet;
using Xunit.Abstractions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json; 

namespace MyBank.ContractTests
{
    public class SaqueContractTests
    {
        private readonly IPactBuilderV3 _pactBuilder;

        public SaqueContractTests(ITestOutputHelper output)
        {
            var config = new PactConfig
            {
                PactDir = "../../../pacts",
                Outputters = new[] { new XUnitOutput(output) },
                DefaultJsonSettings = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }
            };

            var pact = Pact.V3("MyBankWebClient", "MyBankApi", config);
            _pactBuilder = pact.WithHttpInteractions();
        }

        [Fact]
        public async Task DeveRealizarSaqueComSucesso()
        {
            _pactBuilder
                .UponReceiving("Uma requisicao de saque valido")
                    .Given("Existe saldo suficiente na conta")
                    .WithRequest(HttpMethod.Post, "/saque")
                    .WithJsonBody(new { contaId = 1, valor = 100.00 }) 
                .WillRespond()
                    .WithStatus(HttpStatusCode.OK)
                    .WithJsonBody(new { mensagem = "Saque realizado com sucesso" });

            await _pactBuilder.VerifyAsync(async ctx =>
            {
                var client = new HttpClient { BaseAddress = ctx.MockServerUri };

                // simula frontend
                var response = await client.PostAsJsonAsync("/saque", new { contaId = 1, valor = 100.00 });

                // valida se o status code bateu
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            });
        }
    }
}