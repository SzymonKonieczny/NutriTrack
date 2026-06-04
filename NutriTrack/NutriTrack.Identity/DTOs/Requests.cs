namespace NutriTrack.Identity.DTOs;

public record RegisterRequest(
    string Email,
    string Password,
    string? Name
);

public record LoginRequest(
    string Email,
    string Password
);

public record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken
);
