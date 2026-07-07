namespace SocialHub.Domain.Users;
 
/// <summary>Roadmap 5.5 (Theme preferences). Purely a client-side rendering hint — the backend does not act on this value.</summary>
public enum ThemePreference
{
    Light = 0,
    Dark = 1,
    System = 2
}