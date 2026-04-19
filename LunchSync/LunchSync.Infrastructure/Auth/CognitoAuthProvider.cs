using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using LunchSync.Core.Exceptions;
using LunchSync.Core.Modules.Auth;
using LunchSync.Core.Modules.Auth.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LunchSync.Infrastructure.Auth;

public sealed class CognitoAuthProvider : ICognitoAuthProvider
{
    private readonly IAmazonCognitoIdentityProvider _cognitoClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CognitoAuthProvider> _logger;

    public CognitoAuthProvider(
        IAmazonCognitoIdentityProvider cognitoClient,
        IConfiguration configuration,
        ILogger<CognitoAuthProvider> logger)
    {
        _cognitoClient = cognitoClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<CognitoRegisterResult> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        LogCognitoConfigSnapshot("register");

        try
        {
            var response = await _cognitoClient.SignUpAsync(new SignUpRequest
            {
                ClientId = GetRequiredConfig("Cognito:ClientId"),
                SecretHash = BuildSecretHash(request.Email),
                Username = request.Email.Trim().ToLowerInvariant(),
                Password = request.Password,
                UserAttributes = BuildUserAttributes(request)
            }, cancellationToken);

            if (string.IsNullOrWhiteSpace(response.UserSub))
            {
                throw new InvalidOperationException("Cognito register did not return user sub.");
            }

            return new CognitoRegisterResult(
                response.UserSub,
                request.Email.Trim().ToLowerInvariant(),
                request.FullName?.Trim());
        }
        catch (Exception ex) when (TryMapCognitoException(ex, request.Email, out var mappedException))
        {
            throw mappedException;
        }
    }

    public async Task<CognitoLoginResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        LogCognitoConfigSnapshot("login");

        try
        {
            var response = await _cognitoClient.InitiateAuthAsync(new InitiateAuthRequest
            {
                AuthFlow = ResolveAuthFlow(),
                ClientId = GetRequiredConfig("Cognito:ClientId"),
                AuthParameters = BuildAuthParameters(request)
            }, cancellationToken);

            var authResult = response.AuthenticationResult
                ?? throw new InvalidOperationException("Cognito login did not return authentication result.");

            if (string.IsNullOrWhiteSpace(authResult.AccessToken))
            {
                throw new InvalidOperationException("Cognito login did not return access token.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var accessJwt = tokenHandler.ReadJwtToken(authResult.AccessToken);
            var cognitoSub = accessJwt.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value;

            string? email = null;
            string? fullName = null;
            if (!string.IsNullOrWhiteSpace(authResult.IdToken))
            {
                var idJwt = tokenHandler.ReadJwtToken(authResult.IdToken);
                email = idJwt.Claims.FirstOrDefault(claim => claim.Type == "email")?.Value;
                fullName = idJwt.Claims.FirstOrDefault(claim => claim.Type == "name")?.Value;
            }

            email ??= request.Email.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(cognitoSub) || string.IsNullOrWhiteSpace(email))
            {
                throw new InvalidOperationException("Cognito access token is missing required claims.");
            }

            return new CognitoLoginResult(
                authResult.AccessToken,
                authResult.ExpiresIn ?? 0,
                cognitoSub,
                email.Trim().ToLowerInvariant(),
                fullName?.Trim());
        }
        catch (Exception ex) when (TryMapCognitoException(ex, request.Email, out var mappedException))
        {
            throw mappedException;
        }
    }

    public async Task ConfirmSignUpAsync(
        VerifyOtpRequest request,
        CancellationToken cancellationToken = default)
    {
        LogCognitoConfigSnapshot("confirm-sign-up");

        try
        {
            await _cognitoClient.ConfirmSignUpAsync(new ConfirmSignUpRequest
            {
                ClientId = GetRequiredConfig("Cognito:ClientId"),
                SecretHash = BuildSecretHash(request.Email),
                Username = request.Email.Trim().ToLowerInvariant(),
                ConfirmationCode = request.Otp.Trim()
            }, cancellationToken);
        }
        catch (Exception ex) when (TryMapCognitoException(ex, request.Email, out var mappedException))
        {
            throw mappedException;
        }
    }

    public async Task ResendConfirmationCodeAsync(
        ResendOtpRequest request,
        CancellationToken cancellationToken = default)
    {
        LogCognitoConfigSnapshot("resend-confirmation-code");

        try
        {
            await _cognitoClient.ResendConfirmationCodeAsync(new ResendConfirmationCodeRequest
            {
                ClientId = GetRequiredConfig("Cognito:ClientId"),
                SecretHash = BuildSecretHash(request.Email),
                Username = request.Email.Trim().ToLowerInvariant()
            }, cancellationToken);
        }
        catch (Exception ex) when (TryMapCognitoException(ex, request.Email, out var mappedException))
        {
            throw mappedException;
        }
    }

    private AuthFlowType ResolveAuthFlow()
    {
        var configuredFlow = _configuration["Cognito:AuthFlow"];
        return string.IsNullOrWhiteSpace(configuredFlow)
            ? AuthFlowType.USER_PASSWORD_AUTH
            : new AuthFlowType(configuredFlow);
    }

    private static bool TryMapCognitoException(Exception exception, string? email, out Exception mappedException)
    {
        mappedException = exception switch
        {
            UsernameExistsException => new DuplicateEntityException("User", "email", email ?? "unknown"),
            NotAuthorizedException => new InvalidCredentialsException(),
            UserNotFoundException => new InvalidCredentialsException(),
            CodeMismatchException ex => new ValidationException(
                "Ma OTP khong dung.",
                new Dictionary<string, string> { ["otp"] = ex.Message ?? "Ma OTP khong chinh xac." }),
            ExpiredCodeException ex => new ValidationException(
                "Ma OTP da het han.",
                new Dictionary<string, string> { ["otp"] = ex.Message ?? "Ma OTP da het han, vui long yeu cau ma moi." }),
            LimitExceededException ex => new ValidationException(
                "Ban da yeu cau qua nhieu lan.",
                new Dictionary<string, string> { ["otp"] = ex.Message ?? "Vui long doi it phut roi thu lai." }),
            UserNotConfirmedException => new ValidationException(
                "Tai khoan chua duoc xac nhan.",
                new Dictionary<string, string> { ["email"] = "Vui long xac nhan email truoc khi dang nhap." }),
            InvalidPasswordException ex => new ValidationException(
                "Mat khau khong hop le.",
                new Dictionary<string, string> { ["password"] = ex.Message ?? "Mat khau khong dap ung policy cua Cognito." }),
            InvalidParameterException ex => new ValidationException(
                "Du lieu gui len khong hop le.",
                new Dictionary<string, string> { ["auth"] = ex.Message ?? "Yeu cau khong hop le." }),
            TooManyRequestsException ex => new ValidationException(
                "Ban da gui qua nhieu yeu cau.",
                new Dictionary<string, string> { ["auth"] = ex.Message ?? "Vui long doi it phut roi thu lai." }),
            AmazonCognitoIdentityProviderException ex => new InvalidOperationException(ex.Message ?? "Cognito request failed."),
            _ => exception
        };

        return !ReferenceEquals(mappedException, exception);
    }

    private Dictionary<string, string> BuildAuthParameters(LoginRequest request)
    {
        var authParameters = new Dictionary<string, string>
        {
            ["USERNAME"] = request.Email.Trim().ToLowerInvariant(),
            ["PASSWORD"] = request.Password
        };

        var secretHash = BuildSecretHash(request.Email);
        if (!string.IsNullOrWhiteSpace(secretHash))
        {
            authParameters["SECRET_HASH"] = secretHash;
        }

        return authParameters;
    }

    private List<AttributeType> BuildUserAttributes(RegisterRequest request)
    {
        var attributes = new List<AttributeType>
        {
            new()
            {
                Name = "email",
                Value = request.Email.Trim().ToLowerInvariant()
            }
        };

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            attributes.Add(new AttributeType
            {
                Name = "name",
                Value = request.FullName.Trim()
            });
        }

        return attributes;
    }

    private string? BuildSecretHash(string username)
    {
        var clientSecret = _configuration["Cognito:ClientSecret"];
        var clientId = _configuration["Cognito:ClientId"];
        if (string.IsNullOrWhiteSpace(clientSecret) || string.IsNullOrWhiteSpace(clientId))
        {
            return null;
        }

        // Secret hash can cho app client co secret tren Cognito.
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(clientSecret));
        var data = Encoding.UTF8.GetBytes($"{username.Trim().ToLowerInvariant()}{clientId}");
        return Convert.ToBase64String(hmac.ComputeHash(data));
    }

    private string GetRequiredConfig(string key)
    {
        var value = _configuration[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing configuration: {key}");
        }

        return value;
    }

    private void LogCognitoConfigSnapshot(string operation)
    {
        _logger.LogInformation(
            "Cognito config snapshot. Operation={Operation}, Region={Region}, ClientId={ClientId}, AuthFlow={AuthFlow}, HasClientSecret={HasClientSecret}",
            operation,
            _configuration["AWS:Region"],
            _configuration["Cognito:ClientId"],
            _configuration["Cognito:AuthFlow"],
            !string.IsNullOrWhiteSpace(_configuration["Cognito:ClientSecret"]));
    }
}
