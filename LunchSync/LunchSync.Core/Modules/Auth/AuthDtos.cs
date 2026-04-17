namespace LunchSync.Core.Modules.Auth;

public sealed record CurrentUserResponse(
    Guid? UserId,
    string? CognitoSub,
    string? Email,
    string? FullName,
    string? Role,
    bool IsActive
);
