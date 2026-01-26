namespace MyBank.Domain;

public interface IContaRepository
{
    Conta? GetById(int id);
    void Update(Conta conta);
}