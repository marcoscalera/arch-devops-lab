namespace MyBank.Domain;

public class Conta
{
    public int Id { get; private set; }
    public decimal Saldo { get; private set; }
    public bool IsVip { get; private set; }

    public Conta(int id, decimal saldoInicial, bool isVip)
    {
        Id = id;
        Saldo = saldoInicial;
        IsVip = isVip;
    }

    public void Debitar(decimal valor)
    {
        if (valor <= 0) throw new ArgumentException("Valor deve ser maior que zero");
        
        decimal taxa = IsVip ? 0 : 5.00m;
        decimal total = valor + taxa;

        if (total > Saldo)
            throw new InvalidOperationException("Saldo Insuficiente");

        Saldo -= total;
    }
}