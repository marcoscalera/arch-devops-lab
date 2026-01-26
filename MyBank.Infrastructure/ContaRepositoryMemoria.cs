using MyBank.Domain;

namespace MyBank.Infrastructure;

public class ContaRepositoryMemoria : IContaRepository
{
    private readonly List<Conta> _db = new()
    {
        new Conta(1, 1000m, isVip: true), 
        new Conta(2, 500m, isVip: false) 
    };

    public Conta? GetById(int id)
    {
        return _db.FirstOrDefault(c => c.Id == id);
    }

    public void Update(Conta conta)
    {
        var index = _db.FindIndex(c => c.Id == conta.Id);
        if (index != -1) _db[index] = conta;
    }
}