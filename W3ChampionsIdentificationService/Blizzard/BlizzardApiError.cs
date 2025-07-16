using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace W3ChampionsIdentificationService.Blizzard;

public class BlizzardApiError
{
    public class ErrorCode(string code, string description, bool isRetryable)
    {
        public string Code { get; } = code;
        public string Description { get; } = description;
        public bool IsRetryable { get; } = isRetryable;

        public static ErrorCode GeneralError = new("BLZBTNSHR10010101", "General error or the client has been throttled due to high traffic. Check the current error message.", true);
        public static ErrorCode InputParameterMissing = new("BLZBTNSHR10010102", "An input parameter was either missing or malformed. See the 'error' response for details.", false);
        public static ErrorCode NoBattleNetAccountFound = new("BLZBTNSPS10000001", "No BattleNet Account can be found.", false);

        private static readonly Dictionary<string, ErrorCode> _errorCodes = new()
        {
            { GeneralError.Code, GeneralError },
            { InputParameterMissing.Code, InputParameterMissing },
            { NoBattleNetAccountFound.Code, NoBattleNetAccountFound },
        };

        public static ErrorCode FromCode(string code) => _errorCodes.TryGetValue(code, out var errorCode) ? errorCode : new ErrorCode(code, $"Unknown error: {code}", false);
    }

    [JsonConverter(typeof(ErrorCodeJsonConverter))]
    public ErrorCode code { get; }
    public string message { get; }
    public string supplementalInfo { get; }
    public string errorHierarchy { get; }
    public string category { get; }
}

public class ErrorCodeJsonConverter : JsonConverter<BlizzardApiError.ErrorCode>
{
    public override BlizzardApiError.ErrorCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {

        if (reader.TokenType == JsonTokenType.String)
        {
            var errorCodeString = reader.GetString();
            return BlizzardApiError.ErrorCode.FromCode(errorCodeString);
        }

        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        throw new JsonException($"Unexpected token type: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, BlizzardApiError.ErrorCode value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value.Code);
        }
    }
}
