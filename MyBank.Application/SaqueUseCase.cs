using MyBank.Domain;
using System.Collections.Concurrent; // Adicione este using

namespace MyBank.Application;

public class SaqueUseCase
{
    private readonly IContaRepository _repository;
    
    private static readonly ConcurrentDictionary<int, object> _locks = new();

    public SaqueUseCase(IContaRepository repository)
    {
        _repository = repository;
    }

    public SaqueResponse Executar(SaqueRequest request)
    {
        if (request.Valor <= 0)
        {
            throw new ArgumentException("O valor do saque deve ser maior que zero.", nameof(request.Valor));
        }

        var syncLock = _locks.GetOrAdd(request.ContaId, _ => new object());

        lock (syncLock)
        {
            var conta = _repository.GetById(request.ContaId);
            
            if (conta == null)
            {
                throw new InvalidOperationException($"Conta {request.ContaId} não encontrada.");
            }

            conta.Sacar(request.Valor);
            _repository.Update(conta);
            
            return new SaqueResponse(
                conta.Saldo,
                "Saque realizado com sucesso"
            );
        }
    }
}

public record SaqueRequest(int ContaId, decimal Valor);
public record SaqueResponse(decimal NovoSaldo, string Mensagem);