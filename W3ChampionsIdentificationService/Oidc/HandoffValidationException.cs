using System;

namespace W3ChampionsIdentificationService.Oidc;

/// <summary>
/// Thrown when a w3c handoff JWT fails validation (missing, malformed, expired, forged, or tampered).
/// </summary>
public class HandoffValidationException(string message, Exception inner = null) : Exception(message, inner)
{
}
