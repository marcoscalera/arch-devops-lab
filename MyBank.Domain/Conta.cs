namespace MyBank.Domain;

public class Conta
{
    public int Id { get; private set; }
    public decimal Saldo { get; private set; }
    public bool IsVip { get; private set; }

    public Conta(int id, decimal saldo, bool isVip)
    {
        Id = id;
        Saldo = saldo;
        IsVip = isVip;
    }

    public void Sacar(decimal valor)
    {
        if (!IsVip) valor += 5; 
        if (Saldo < valor) throw new InvalidOperationException("Saldo insuficiente");
        Saldo -= valor;
    }
    
    public void DefinirSaldo(decimal novoSaldo)
    {
        Saldo = novoSaldo;
    }
}