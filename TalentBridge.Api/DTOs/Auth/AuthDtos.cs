namespace TalentBridge.Api.DTOs.Auth;

public record RegisterDto(
    string FullName,
    string Email,
    string Password,
    string Role // "Candidate" or "Recruiter"
);

public record LoginDto(
    string Email,
    string Password
);

public record AuthResponseDto(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    string UserId,
    string Email,
    string FullName,
    string Role
);

public record RefreshTokenDto(
    string AccessToken
);
