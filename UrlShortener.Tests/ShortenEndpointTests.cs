using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class ShortenEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ShortenEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ShortenEndpoint_ReturnsOk_ForValidUrl()
    {
        // Arrange
        var validUrl = "https://www.microsoft.com";
        var requestUri = $"/shorten?url={validUrl}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsStringAsync();
        Assert.Contains("/go/", result); // Verifica que a resposta contém a URL encurtada
    }

    [Fact]
    public async Task ShortenEndpoint_ReturnsBadRequest_ForInvalidUrl()
    {
        // Arrange
        var invalidUrl = "invalid-url";
        var requestUri = $"/shorten?url={invalidUrl}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadAsStringAsync();
        Assert.Contains("The URL query string is required", result);
    }
}
