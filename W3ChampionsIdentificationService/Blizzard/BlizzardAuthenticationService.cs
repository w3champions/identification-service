using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;

namespace W3ChampionsIdentificationService.Blizzard;

public class BlizzardAuthenticationService : IBlizzardAuthenticationService
{
    private readonly string _bnetClientId = Environment.GetEnvironmentVariable("BNET_API_CLIENT_ID");
    private readonly string _bnetApiSecret = Environment.GetEnvironmentVariable("BNET_API_SECRET");

    private const string StreamingProviderEndpoint = "/StreamingProviderService/v1/GetPlayableTitles";
    private const string UserInfoEndpoint = "/oauth/userinfo";
    private const string TokenEndpoint = "/oauth/token";

    public async Task<BlizzardUserInfo> GetUser(string bearer, BnetRegion region)
    {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri($"{GetAuthenticationUri(region)}{UserInfoEndpoint}");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearer);

        var res = await httpClient.GetAsync("");
        if (!res.IsSuccessStatusCode)
        {
            var errorContent = await res.Content.ReadAsStringAsync();
            Log.Error("Failed to get user info from Blizzard: [{ErrorCode}] {response}", res.StatusCode, errorContent);
            return null;
        }

        var content = await res.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<BlizzardUserInfo>(content);
    }

    public async Task<OAuthToken> GetToken(string code, string redirectUri, BnetRegion region)
    {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri($"{GetAuthenticationUri(region)}{TokenEndpoint}");
        var res = await httpClient.PostAsync($"?code={code}&grant_type=authorization_code&redirect_uri={redirectUri}&client_id={_bnetClientId}&client_secret={_bnetApiSecret}", null);
        if (!res.IsSuccessStatusCode)
        {
            var errorContent = await res.Content.ReadAsStringAsync();
            Log.Error("Failed to get token from Blizzard: [{ErrorCode}] {response}", res.StatusCode, errorContent);
            return null;
        }

        var content = await res.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OAuthToken>(content);
    }

    public async Task<(List<BlizzardPlayableTitle> titles, AuthenticationError error)> GetPlayableTitles(OAuthToken token, BnetRegion region)
    {
        if (!token.SupportsScopes)
        {
            Log.Information("OAuth token does not support scopes - old client?");
            return (null, AuthenticationError.UnsupportedVersion());
        }
        if (!token.HasScope("openid"))
        {
            Log.Error("Received OAuth token with scopes '{Scopes}' but without openid scope - something is wrong!", token.scope);
            return (null, AuthenticationError.UnknownError());
        }
        if (!token.HasScope("streaming.titles"))
        {
            Log.Information("User has opted out of streaming.titles scope. Scopes: '{Scopes}'", token.scope);
            return (null, AuthenticationError.MissingPlayableTitlesScope());
        }

        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri($"{GetPartnerUri(region)}{StreamingProviderEndpoint}");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        var requestContent = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        var res = await httpClient.PostAsync("", requestContent);
        if (!res.IsSuccessStatusCode)
        {
            var errorContent = await res.Content.ReadAsStringAsync();
            Log.Error("Failed to get playable titles from Blizzard: [{ErrorCode}] {response}", res.StatusCode, errorContent);
            return (null, AuthenticationError.ApiCallFailed());
        }

        var content = await res.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<GetPlayableTitlesResponse>(content);

        if (apiResponse?.error != null)
        {
            Log.Error("Failed to get playable titles from Blizzard: [{ErrorCode}] {ErrorDescription}", apiResponse.error.code?.Code, apiResponse.error.code?.Description);
            return (null, AuthenticationError.ApiCallFailed());
        }

        if (apiResponse?.titleIds == null || apiResponse.titleIds.Length == 0)
        {
            return (new List<BlizzardPlayableTitle>(), null);
        }

        return (BlizzardPlayableTitleExtensions.FromTitleIds(apiResponse.titleIds), null);
    }

    private static string GetAuthenticationUri(BnetRegion bnetRegion)
    {
        switch (bnetRegion)
        {
            case BnetRegion.eu:
                return "https://oauth.battle.net";
            case BnetRegion.cn:
                return "https://oauth.battlenet.com.cn";
            default:
                return "https://oauth.battle.net";
        }
    }

    private static string GetPartnerUri(BnetRegion bnetRegion)
    {
        // The US endpoint is the only endpoint for the entire global world.
        switch (bnetRegion)
        {
            case BnetRegion.eu:
                return "https://partner-us.api.blizzard.com";
            case BnetRegion.cn:
                return "https://partner-us.api.blizzard.com"; // TODO: Get china endpoint
            default:
                return "https://partner-us.api.blizzard.com";
        }
    }
}

public enum BnetRegion
{
    none, eu, cn
}
