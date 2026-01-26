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
        // arrange
        var mockRepo = new Mock<IContaRepository>();
        var contaMock = new Conta(1, 1000m, true);
        
        mockRepo.Setup(r => r.GetById(1)).Returns(contaMock);

        var useCase = new SaqueUseCase(mockRepo.Object);

        // act
        useCase.Executar(new SaqueRequest(1, 100m));

        // assert
        mockRepo.Verify(r => r.Update(It.IsAny<Conta>()), Times.Once);
    }
}