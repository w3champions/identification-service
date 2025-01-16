using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace W3ChampionsIdentificationService.Microsoft;

public class AccessToken
{
    public string id_token { get; set; }
}

public class MicrosoftAuthenticationService : IMicrosoftAuthenticationService
{
    private readonly string _clientId = Environment.GetEnvironmentVariable("AAD_APP_CLIENT_ID");
    private readonly string _clientSecret = Environment.GetEnvironmentVariable("AAD_APP_CLIENT_SECRET");

    public async Task<string> GetIdToken(string code, string redirectUri)
    {
        var httpClient = new HttpClient();
        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("scope", "openid"),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", redirectUri),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("client_secret", _clientSecret),
        });
        var res = await httpClient.PostAsync("https://login.microsoftonline.com/consumers/oauth2/v2.0/token", form);
        if (!res.IsSuccessStatusCode)
        {
            return null;
        }

        var readAsStringAsync = await res.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AccessToken>(readAsStringAsync).id_token;
    }

    public async Task<MicrosoftUser> GetUser(string idToken)
    {
        var token = (SecurityToken)new JwtSecurityToken();
        string authority = "https://login.microsoftonline.com/consumers/v2.0/";
        string consumer_user_issuer = "https://login.microsoftonline.com/9188040d-6c67-4c5b-b112-36a304b66dad/v2.0";

        IConfigurationManager<OpenIdConnectConfiguration> configurationManager =
            new ConfigurationManager<OpenIdConnectConfiguration>($"{authority}.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
        OpenIdConnectConfiguration openIdConfig = await configurationManager.GetConfigurationAsync(CancellationToken.None);

        var validationParams = new TokenValidationParameters
        {
            ValidIssuer = consumer_user_issuer,
            ValidAudience = _clientId,
            IssuerSigningKeys = openIdConfig.SigningKeys
        };

        var handler = new JwtSecurityTokenHandler();

        handler.ValidateToken(idToken, validationParams, out token);
        return new MicrosoftUser
        {
            sub = (token as JwtSecurityToken).Claims.First(c => c.Type == "sub").Value
        };
    }
}
