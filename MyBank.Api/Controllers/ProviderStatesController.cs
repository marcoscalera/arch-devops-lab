using Microsoft.AspNetCore.Mvc;
using MyBank.Domain;

namespace MyBank.Api.Controllers
{
    /// <summary>
    /// controlador para configurar estados de teste do Pact
    /// </summary>
    [ApiController]
    [Route("provider-states")]
    public class ProviderStatesController : ControllerBase
    {
        private readonly IContaRepository _repository;

        /// <summary>
        /// construtor do ProviderStatesController
        /// </summary>
        public ProviderStatesController(IContaRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// configura o estado do provedor antes da execução do teste
        /// </summary>
        [HttpPost]
        public IActionResult Setup([FromBody] ProviderStateRequest providerState)
        {
            if (providerState.State == "Existe saldo suficiente na conta")
            {
                var existing = _repository.GetById(1);
                if (existing == null)
                {
                    var conta = new Conta(1, 1000m, true);
                    _repository.Add(conta);
                }
            }

            return Ok();
        }
    }

    /// <summary>
    /// DTO para receber o estado do Pact
    /// </summary>
    public class ProviderStateRequest
    {
        /// <summary>
        /// descrição do estado esperado
        /// </summary>
        public string State { get; set; } = string.Empty;
    }
}