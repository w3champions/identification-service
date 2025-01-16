using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace W3ChampionsIdentificationService.Blizzard;

public class BlizzardAuthenticationService : IBlizzardAuthenticationService
{
    private readonly string _bnetClientId = Environment.GetEnvironmentVariable("BNET_API_CLIENT_ID");
    private readonly string _bnetApiSecret = Environment.GetEnvironmentVariable("BNET_API_SECRET");

    public async Task<BlizzardUserInfo> GetUser(string bearer, BnetRegion region)
    {
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri($"{GetAuthenticationUri(region)}/oauth/userinfo");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearer);

        var res = await httpClient.GetAsync("");
        if (!res.IsSuccessStatusCode)
        {
            return null;
        }

        var readAsStringAsync = await res.Content.ReadAsStringAsync();
        return  JsonSerializer.Deserialize<BlizzardUserInfo>(readAsStringAsync);
    }

    public async Task<OAuthToken> GetToken(string code, string redirectUri, BnetRegion region)
    {
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri($"{GetAuthenticationUri(region)}/oauth/token");
        var res = await httpClient.PostAsync($"?code={code}&grant_type=authorization_code&redirect_uri={redirectUri}&client_id={_bnetClientId}&client_secret={_bnetApiSecret}", null);
        if (!res.IsSuccessStatusCode)
        {
            return null;
        }

        var readAsStringAsync = await res.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OAuthToken>(readAsStringAsync);
    }

    private static string GetAuthenticationUri(BnetRegion bnetRegion)
    {
        switch (bnetRegion)
        {
            case BnetRegion.eu:
                return "https://eu.battle.net";
            case BnetRegion.cn:
                return "https://www.battlenet.com.cn";
            default:
                return "https://eu.battle.net";
        }
    }
}

public enum BnetRegion
{
    none, eu, cn
}
