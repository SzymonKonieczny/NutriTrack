namespace NutriTrack.Identity.DTOs;

public record AuthResponse(
    string UserId,
    string Email,
    string? Name,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc
);

public record ErrorResponse(
    string Message,
    IReadOnlyList<string>? Errors = null
);
