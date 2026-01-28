using MyBank.Domain;

namespace MyBank.Application;

public class SaqueUseCase
{
    private readonly IContaRepository _repository;

    public SaqueUseCase(IContaRepository repository)
    {
        _repository = repository;
    }

    public SaqueResponse Executar(SaqueRequest req)
    {
        var conta = _repository.GetById(req.ContaId);
        
        if (conta == null) throw new ArgumentException("Conta não encontrada");

        conta.Sacar(req.Valor);

        _repository.Update(conta);

        return new SaqueResponse(conta.Saldo, "Saque realizado com sucesso");
    }
}

public record SaqueRequest(int ContaId, decimal Valor);
public record SaqueResponse(decimal NovoSaldo, string Mensagem);