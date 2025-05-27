using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;

namespace W3ChampionsIdentificationService.Twitch;

public class TwitchAuthenticationService : ITwitchAuthenticationService
{
    private OAuthToken _cachedToken { get; set; }

    private readonly string _twitchApiSecret = Environment.GetEnvironmentVariable("TWITCH_API_SECRET");

    public async Task<OAuthToken> GetToken()
    {
        // Twitch token expires after 60 days, so this cache will save many calls to the twitch API
        if (Cache.TwitchToken != null && !Cache.TwitchToken.hasExpired())
        {
            return Cache.TwitchToken;
        }

        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://id.twitch.tv/oauth2/token");
        var result = await httpClient.PostAsync($"?client_id=38ac0gifyt5khcuq23h2p8zpcqosbc&client_secret={_twitchApiSecret}&grant_type=client_credentials", null);
        if (result.StatusCode == HttpStatusCode.OK)
        {
            var content = await result.Content.ReadAsStringAsync();
            _cachedToken = JsonSerializer.Deserialize<OAuthToken>(content);
            _cachedToken.CreateDate = DateTime.Now;
            Cache.TwitchToken = _cachedToken;
            return _cachedToken;
        }

        Log.Error("Could not retrieve Twitch Token: {StatusCode}", result.StatusCode);
        throw new Exception("Could not retrieve Twitch Token");
    }

}

public static class Cache
{
    public static OAuthToken TwitchToken { get; set; }
}
