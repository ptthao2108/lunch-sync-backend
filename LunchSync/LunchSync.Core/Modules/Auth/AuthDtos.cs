using System.Text.Json.Serialization;

namespace LunchSync.Core.Modules.Auth;

public sealed record CurrentUserResponse(
    [property: JsonPropertyName("user_id")] Guid? UserId,
    [property: JsonPropertyName("cognito_sub")] string? CognitoSub,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("full_name")] string? FullName,
    [property: JsonPropertyName("role")] string? Role,
    [property: JsonPropertyName("is_active")] bool IsActive
);
