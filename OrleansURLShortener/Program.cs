// <Configuration>
using Orleans.Runtime;
using System.IO;

var builder = WebApplication.CreateBuilder();

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.UseLocalhostClustering();
    siloBuilder.AddMemoryGrainStorage("urls");
    siloBuilder.UseDashboard(options => { });
});

var app = builder.Build();
// </Configuration>

// <Endpoints>
app.MapGet("/", () => "Welcome to the URL shortener, powered by Orleans!");

app.MapGet("/shorten",
    async (IGrainFactory grains, HttpRequest request, string url) =>
    {
        var host = $"{request.Scheme}://{request.Host.Value}";

        // Validate the URL query string.
        if (string.IsNullOrWhiteSpace(url) ||
            Uri.IsWellFormedUriString(url, UriKind.Absolute) is false)
        {
            return Results.BadRequest($"""
                The URL query string is required and needs to be well formed.
                Consider, ${host}/shorten?url=https://www.microsoft.com.
                """);
        }

        // Create a unique, short ID
        var shortenedRouteSegment = Guid.NewGuid().GetHashCode().ToString("X");

        // Create and persist a grain with the shortened ID and full URL
        var shortenerGrain =
            grains.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);

        await shortenerGrain.SetUrl(url);

        // Return the shortened URL for later use
        var resultBuilder = new UriBuilder(host)
        {
            Path = $"/go/{shortenedRouteSegment}"
        };

        return Results.Ok(resultBuilder.Uri);
    });

app.MapGet("/go/{shortenedRouteSegment}",
    async (IGrainFactory grains, string shortenedRouteSegment) =>
    {
        // Retrieve the grain using the shortened ID and redirect to the original URL        
        var shortenerGrain = grains.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);
        var url = await shortenerGrain.GetUrl();

        return Results.Redirect(url);
    });

app.Run();
// </Endpoints>

// <GrainInterface>
public interface IUrlShortenerGrain : IGrainWithStringKey
{
    Task SetUrl(string fullUrl);
    Task<string> GetUrl();

    Task Message();
}
// </GrainInterface>

// <Grain>
public class UrlShortenerGrain : Grain, IUrlShortenerGrain
{
    private readonly IPersistentState<UrlDetails> _state;

    public UrlShortenerGrain(
        [PersistentState(
                stateName: "url",
                storageName: "urls")]
                IPersistentState<UrlDetails> state)
    {
        _state = state;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task SetUrl(string fullUrl)
    {
        _state.State = new UrlDetails() { ShortenedRouteSegment = this.GetPrimaryKeyString(), FullUrl = fullUrl };
        await _state.WriteStateAsync();
    }

    public Task<string> GetUrl()
    {
        return Task.FromResult(_state.State.FullUrl);
    }

    public Task Message()
    {
        Console.WriteLine("teste");
        return Task.CompletedTask;
    }
}

[GenerateSerializer]
public record UrlDetails
{
    public string FullUrl { get; set; } = String.Empty;
    
    public string ShortenedRouteSegment { get; set; } = String.Empty;
}
// </Grain>

public partial class Program { }
