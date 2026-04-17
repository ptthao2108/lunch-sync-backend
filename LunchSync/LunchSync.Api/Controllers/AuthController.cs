using LunchSync.Core.Common.Auth;
using LunchSync.Core.Common.Interfaces;
using LunchSync.Core.Modules.Auth;
using LunchSync.Core.Modules.Auth.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LunchSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUserRepository _userRepository;

    public AuthController(
        ICurrentUserService currentUser,
        IUserRepository userRepository)
    {
        _currentUser = currentUser;
        _userRepository = userRepository;
    }

    [Authorize(Policy = AuthPolicies.CognitoUser)]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Unauthorized();
        }

        var localUser = string.IsNullOrWhiteSpace(_currentUser.CognitoSub)
            ? null
            : await _userRepository.GetByCognitoSubAsync(_currentUser.CognitoSub, cancellationToken);

        return Ok(new CurrentUserResponse(
            localUser?.Id,
            _currentUser.CognitoSub,
            localUser?.Email ?? _currentUser.Email,
            localUser?.FullName ?? _currentUser.Name,
            localUser?.Role.ToString().ToLowerInvariant() ?? _currentUser.Role,
            localUser?.IsActive ?? _currentUser.IsActive));
    }
}
