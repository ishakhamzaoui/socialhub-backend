namespace SocialHub.API.Contracts.Auth;
 
// Thin request DTOs for endpoints whose commands carry server-derived data
// (IP address) that must never be bound directly from client input.
 
public sealed record LoginRequest(string Email, string Password);
 
public sealed record RefreshTokenRequest(string RefreshToken);
 
public sealed record LogoutRequest(string RefreshToken);