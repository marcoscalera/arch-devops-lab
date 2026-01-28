using FluentAssertions;
using Moq;
using MyBank.Application;
using MyBank.Domain;
using Xunit;

namespace MyBank.UnitTests;

public class SaqueUseCaseTests
{
    [Fact]
    public void Deve_Chamar_Update_No_Repository_Quando_Sucesso()
    {
        var mockRepo = new Mock<IContaRepository>();
        var contaMock = new Conta(1, 1000m, true);
        
        mockRepo.Setup(r => r.GetById(1)).Returns(contaMock);

        var useCase = new SaqueUseCase(mockRepo.Object);

        useCase.Executar(new SaqueRequest(1, 100m));

        mockRepo.Verify(r => r.Update(It.IsAny<Conta>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    [InlineData(-100.50)]
    public void Deve_Rejeitar_Valores_Invalidos(decimal valorInvalido)
    {
        var mockRepo = new Mock<IContaRepository>();
        var conta = new Conta(1, 1000m, true);
        mockRepo.Setup(r => r.GetById(1)).Returns(conta);
        
        var useCase = new SaqueUseCase(mockRepo.Object);

        Action act = () => useCase.Executar(new SaqueRequest(1, valorInvalido));

        act.Should().Throw<ArgumentException>()
            .WithMessage("*valor*");
    }

    [Fact]
    public void Deve_Rejeitar_Conta_Inexistente()
    {
        var mockRepo = new Mock<IContaRepository>();
        mockRepo.Setup(r => r.GetById(999)).Returns((Conta?)null);
        
        var useCase = new SaqueUseCase(mockRepo.Object);

        Action act = () => useCase.Executar(new SaqueRequest(999, 100m));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*não encontrada*");
    }

    [Fact]
    public void Cliente_VIP_Nao_Deve_Pagar_Taxa()
    {
        var mockRepo = new Mock<IContaRepository>();
        var contaVip = new Conta(1, 1000m, isVip: true);
        mockRepo.Setup(r => r.GetById(1)).Returns(contaVip);
        
        var useCase = new SaqueUseCase(mockRepo.Object);

        var response = useCase.Executar(new SaqueRequest(1, 100m));

        response.NovoSaldo.Should().Be(900m);
    }

    [Fact]
    public void Cliente_Comum_Deve_Pagar_Taxa_De_5_Reais()
    {
        var mockRepo = new Mock<IContaRepository>();
        var contaComum = new Conta(1, 1000m, isVip: false);
        mockRepo.Setup(r => r.GetById(1)).Returns(contaComum);
        
        var useCase = new SaqueUseCase(mockRepo.Object);

        var response = useCase.Executar(new SaqueRequest(1, 100m));

        response.NovoSaldo.Should().Be(895m);
    }

    [Fact]
    public void Deve_Rejeitar_Saque_Com_Saldo_Insuficiente()
    {
        var mockRepo = new Mock<IContaRepository>();
        var conta = new Conta(1, 50m, true);
        mockRepo.Setup(r => r.GetById(1)).Returns(conta);
        
        var useCase = new SaqueUseCase(mockRepo.Object);

        Action act = () => useCase.Executar(new SaqueRequest(1, 100m));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*insuficiente*");
    }

    [Fact]
    public void Deve_Permitir_Saque_Do_Saldo_Total_Para_VIP()
    {
        var mockRepo = new Mock<IContaRepository>();
        var conta = new Conta(1, 1000m, isVip: true);
        mockRepo.Setup(r => r.GetById(1)).Returns(conta);
        
        var useCase = new SaqueUseCase(mockRepo.Object);

        var response = useCase.Executar(new SaqueRequest(1, 1000m));

        response.NovoSaldo.Should().Be(0m);
        response.Mensagem.Should().Contain("sucesso");
    }

    [Fact]
    public void Deve_Calcular_Taxa_Antes_De_Verificar_Saldo()
    {
        var mockRepo = new Mock<IContaRepository>();
        var conta = new Conta(1, 100m, isVip: false);
        mockRepo.Setup(r => r.GetById(1)).Returns(conta);
        
        var useCase = new SaqueUseCase(mockRepo.Object);

        Action act = () => useCase.Executar(new SaqueRequest(1, 96m));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*insuficiente*");
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(999.99)]
    public void Deve_Aceitar_Valores_Decimais_Validos(decimal valor)
    {
        var mockRepo = new Mock<IContaRepository>();
        var conta = new Conta(1, 1000m, isVip: true);
        mockRepo.Setup(r => r.GetById(1)).Returns(conta);
        
        var useCase = new SaqueUseCase(mockRepo.Object);

        Action act = () => useCase.Executar(new SaqueRequest(1, valor));

        act.Should().NotThrow();
    }

    [Fact]
    public void Deve_Chamar_GetById_Exatamente_Uma_Vez()
    {
        var mockRepo = new Mock<IContaRepository>();
        var conta = new Conta(1, 1000m, true);
        mockRepo.Setup(r => r.GetById(1)).Returns(conta);
        
        var useCase = new SaqueUseCase(mockRepo.Object);

        useCase.Executar(new SaqueRequest(1, 100m));

        mockRepo.Verify(r => r.GetById(1), Times.Once);
    }

    [Fact]
    public void Nao_Deve_Chamar_Update_Se_GetById_Retorna_Null()
    {
        var mockRepo = new Mock<IContaRepository>();
        mockRepo.Setup(r => r.GetById(999)).Returns((Conta?)null);
        
        var useCase = new SaqueUseCase(mockRepo.Object);

        try
        {
            useCase.Executar(new SaqueRequest(999, 100m));
        }
        catch
        {
        }

        mockRepo.Verify(r => r.Update(It.IsAny<Conta>()), Times.Never);
    }

    [Fact]
    public void Deve_Atualizar_Conta_Com_Saldo_Correto()
    {
        var mockRepo = new Mock<IContaRepository>();
        var conta = new Conta(1, 1000m, isVip: true);
        mockRepo.Setup(r => r.GetById(1)).Returns(conta);
        
        Conta? contaAtualizada = null;
        mockRepo.Setup(r => r.Update(It.IsAny<Conta>()))
            .Callback<Conta>(c => contaAtualizada = c);
        
        var useCase = new SaqueUseCase(mockRepo.Object);

        useCase.Executar(new SaqueRequest(1, 100m));

        contaAtualizada.Should().NotBeNull();
        contaAtualizada!.Saldo.Should().Be(900m);
    }
}