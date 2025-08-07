using System;
using System.Linq;

namespace W3ChampionsIdentificationService;

public class OAuthToken
{
    public string access_token { get; set; }
    public string token_type { get; set; }
    public int expires_in { get; set; }
    public DateTime CreateDate { get; set; }
    public string scope { get; set; }

    public bool hasExpired()
    {
        return DateTime.Now > CreateDate.Add(TimeSpan.FromSeconds(expires_in));
    }

    public bool HasScope(string scope)
    {
        if (!SupportsScopes) return false;
        return scope.Split(' ').Contains(scope);
    }

    public bool SupportsScopes
    {
        get
        {
            return !string.IsNullOrEmpty(scope);
        }
    }
}
