using System;
using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Orleans;
using Xunit;

public class GoEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IGrainFactory> _grainFactoryMock;
    private readonly Mock<IUrlShortenerGrain> _grainMock;

    public GoEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GoEndpoint_RedirectsToOriginalUrl()
    {       
        // Primeiro, salva a URL no estado usando SetUrl
        var validUrl = "https://www.microsoft.com";
        var requestUri = $"/shorten?url={validUrl}";
        var response = await _client.GetAsync(requestUri);
        var result = await response.Content.ReadAsStringAsync();

        // Arrange
        string shortenedRouteSegment = ExtractRoute(result);
        requestUri = $"/go/{shortenedRouteSegment}";

        // Act      
        response = await _client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("https://www.microsoft.com/", response.RequestMessage?.RequestUri?.ToString());
    }

    private string ExtractRoute(string url)
    {
        // Remover as aspas extra (\" no início e no final)
        url = url.Trim('"');

        // Encontrar a posição do último '/' na URL
        int lastSlashIndex = url.LastIndexOf('/');

        // Extrair o valor após o '/'
        string value = url.Substring(lastSlashIndex + 1);

        return value;
    }
}
