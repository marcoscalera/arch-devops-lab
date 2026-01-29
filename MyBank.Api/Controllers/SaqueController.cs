using Microsoft.AspNetCore.Mvc;
using MyBank.Application;

namespace MyBank.Api.Controllers;

/// <summary>
/// Controlador responsável pelas operações de saque financeiro.
/// </summary>
[ApiController]
[Route("[controller]")]
public class SaqueController : ControllerBase
{
    private readonly SaqueUseCase _useCase;

    /// <summary>
    /// Construtor do SaqueController.
    /// </summary>
    /// <param name="useCase">Caso de uso de saque injetado.</param>
    public SaqueController(SaqueUseCase useCase)
    {
        _useCase = useCase;
    }

    /// <summary>
    /// Realiza um saque na conta corrente.
    /// </summary>
    /// <remarks>
    /// **DOCUMENTAÇÃO DE IMPACTO (LEIA ANTES DE ALTERAR):**
    /// 
    /// Esta rota é crítica e consumida por:
    /// 1. **App Mobile (Android/iOS):** Tela de "Saque Rápido".
    /// 2. **Internet Banking (Angular):** Componente `ModalSaque`.
    /// 
    /// **Contrato de Resposta:**
    /// - O campo `novoSaldo` é obrigatório. Se renomear, o App Mobile **trava**.
    /// - O campo `mensagem` é exibido num Toast no Angular.
    /// </remarks>
    /// <response code="200">Saque realizado com sucesso.</response>
    /// <response code="400">Saldo insuficiente ou regra de negócio.</response>
    [HttpPost]
    public IActionResult RealizarSaque([FromBody] SaqueRequest req)
    {
        try
        {
            var response = _useCase.Executar(req);
            return Ok(response);
        }
        catch (ArgumentException ex) { return BadRequest(new { erro = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { erro = ex.Message }); }
        catch (Exception ex) { return Problem(ex.Message); }
    }
}