using PactNet.Verifier;
using Xunit.Abstractions;

namespace MyBank.ContractTests
{
    public class ApiProviderTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly PactVerifier _verifier;
        private readonly string _pactUri = Path.Combine("..", "..", "..", "pacts", "MyBankWebClient-MyBankApi.json");
        private readonly Uri _providerUri = new Uri("http://localhost:5000"); 

        public ApiProviderTests(ITestOutputHelper output)
        {
            _output = output;
            
            var config = new PactVerifierConfig
            {
                Outputters = new[] { new XUnitOutput(_output) }
            };

            _verifier = new PactVerifier("MyBankApi", config);
        }

        [Fact]
        public void DeveAtenderAoContrato()
        {
            _verifier
                .WithHttpEndpoint(_providerUri)
                .WithFileSource(new FileInfo(_pactUri))
                .WithProviderStateUrl(new Uri(_providerUri, "/provider-states")) 
                .Verify();
        }

        public void Dispose()
        {
        }
    }
}