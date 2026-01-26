using MyBank.Domain;

namespace MyBank.Application;

public record SaqueRequest(int ContaId, decimal Valor);
public record SaqueResponse(decimal NovoSaldo, string Mensagem);

public class SaqueUseCase
{
    private readonly IContaRepository _repo;

    public SaqueUseCase(IContaRepository repo)
    {
        _repo = repo;
    }

    public SaqueResponse Executar(SaqueRequest request)
    {
        var conta = _repo.GetById(request.ContaId);
        
        if (conta == null)
            throw new Exception("Conta não encontrada");

        conta.Debitar(request.Valor);

        _repo.Update(conta);

        return new SaqueResponse(conta.Saldo, "Saque realizado com sucesso");
    }
}