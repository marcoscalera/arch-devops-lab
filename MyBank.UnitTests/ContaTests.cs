using MyBank.Domain;
using Xunit;

namespace MyBank.UnitTests;

public class ContaTests
{
    [Fact]
    public void Conta_VIP_Nao_Deve_Cobrar_Taxa()
    {
        // arrange
        var conta = new Conta(1, 100m, isVip: true);
        
        // act
        conta.Debitar(50m);
        
        // assert
        Assert.Equal(50m, conta.Saldo);
    }

    [Fact]
    public void Conta_Comum_Deve_Cobrar_Taxa_De_5_Reais()
    {
        var conta = new Conta(1, 100m, isVip: false);
        conta.Debitar(50m);
        // 100 - 50 - 5 = 45
        Assert.Equal(45m, conta.Saldo);
    }

    [Fact]
    public void Deve_Lancar_Erro_Se_Saldo_Insuficiente()
    {
        var conta = new Conta(1, 10m, isVip: false);
        // não pode
        Assert.Throws<InvalidOperationException>(() => conta.Debitar(10m));
    }
}