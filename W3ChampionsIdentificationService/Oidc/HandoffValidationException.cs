using System;

namespace W3ChampionsIdentificationService.Oidc;

/// <summary>
/// Thrown when a w3c handoff JWT fails validation (missing, malformed, expired, forged, or tampered).
/// </summary>
public class HandoffValidationException : Exception
{
    public HandoffValidationException(string message, Exception inner = null)
        : base(message, inner) { }
}
