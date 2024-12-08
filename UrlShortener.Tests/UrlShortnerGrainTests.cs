using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Moq;
using Orleans;
using Orleans.TestingHost;
using Xunit;

public class UrlShortenerGrainTests : IClassFixture<TestingClusterFixture>
{
    private readonly TestCluster _cluster;

    public UrlShortenerGrainTests(TestingClusterFixture fixture)
    {
        _cluster = fixture.Cluster;
    }

    [Fact]
    public async Task SetUrl_And_GetUrl_WorksCorrectly()
    {
        // Arrange
        var primaryKey = "TestPrimaryKey";
        var testUrl = "https://www.microsoft.com";

        // Obter o proxy do Grain real
        var grain = _cluster.GrainFactory.GetGrain<IUrlShortenerGrain>(primaryKey);

        // Act
        await grain.SetUrl(testUrl);

        var result = await grain.GetUrl();

        // Assert
        Assert.Equal(testUrl, result);
    }

    [Fact]
    public async Task GetUrl_ReturnsSavedUrl()
    {
        // Arrange
        var primaryKey = "TestPrimaryKey";
        var testUrl = "https://www.microsoft.com";

        // Cria um proxy do Grain usando a chave primária
        var grain = _cluster.GrainFactory.GetGrain<IUrlShortenerGrain>(primaryKey);

        // Primeiro, salva a URL no estado usando SetUrl
        await grain.SetUrl(testUrl);

        // Act
        var result = await grain.GetUrl();

        // Assert
        Assert.Equal(testUrl, result);
    }

    [Fact]
    public async Task Message_WorksCorrectly()
    {
        // Arrange
        var primaryKey = "TestPrimaryKey";
        var testUrl = "https://www.microsoft.com";

        // Cria um proxy do Grain usando a chave primária
        var grain = _cluster.GrainFactory.GetGrain<IUrlShortenerGrain>(primaryKey);

        // Act
        await grain.Message();

        // Assert

    }
}

// Configuração do TestingCluster
public class TestingClusterFixture : IDisposable
{
    public TestCluster Cluster { get; }

    public TestingClusterFixture()
    {
        // Configuração do cluster de teste
        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<SiloConfigurator>();
        Cluster = builder.Build();
        Cluster.Deploy();
    }

    public void Dispose()
    {
        Cluster.StopAllSilos();
    }
}

// Configuração do silo Orleans
public class SiloConfigurator : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        // Armazenamento em memória para os Grains
        siloBuilder.AddMemoryGrainStorage("urls");
    }
}
