using MyBank.Domain;

namespace MyBank.UnitTests;

public class ContaTests
{
    [Fact]
    public void Saque_Conta_VIP_Nao_Deve_Cobrar_Taxa()
    {
        var conta = new Conta(1, 1000m, true);

        conta.Sacar(100m); 

        Assert.Equal(900m, conta.Saldo);
    }

    [Fact]
    public void Saque_Conta_Comum_Deve_Cobrar_Taxa()
    {
        var conta = new Conta(2, 1000m, false);

        conta.Sacar(100m); 

        Assert.Equal(895m, conta.Saldo);
    }

    [Fact]
    public void Saque_Sem_Saldo_Deve_Lancar_Erro()
    {
        var conta = new Conta(3, 10m, false);

        Assert.Throws<InvalidOperationException>(() => conta.Sacar(50m));
    }
}