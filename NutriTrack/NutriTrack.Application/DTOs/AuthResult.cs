namespace NutriTrack.Application.DTOs;

/// <summary>
/// Represents the outcome of an authentication operation.
/// Either <see cref="Success"/> with an <see cref="NutriTrack.Identity.DTOs.AuthResponse"/>,
/// or <see cref="Failure"/> with an error message and optional detail collection.
/// </summary>
public sealed record AuthResult
{
    private AuthResult() { }

    /// <summary>The <see cref="NutriTrack.Identity.DTOs.AuthResponse"/> when the operation succeeded, or null.</summary>
    public NutriTrack.Identity.DTOs.AuthResponse? Response { get; private init; }

    /// <summary>True when the operation succeeded.</summary>
    public bool IsSuccess { get; private init; }

    /// <summary>True when the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Human-readable error message. Null on success.</summary>
    public string? ErrorMessage { get; private init; }

    /// <summary>Optional per-field error details. Null on success.</summary>
    public IReadOnlyList<string>? Errors { get; private init; }

    public static AuthResult Ok(NutriTrack.Identity.DTOs.AuthResponse response) =>
        new() { IsSuccess = true, Response = response };

    public static AuthResult Fail(string message, IReadOnlyList<string>? errors = null) =>
        new() { IsSuccess = false, ErrorMessage = message, Errors = errors };
}