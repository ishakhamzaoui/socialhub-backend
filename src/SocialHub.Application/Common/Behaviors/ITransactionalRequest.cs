namespace SocialHub.Application.Common.Behaviors;
 
/// <summary>
/// Implement on a command to have it run inside an explicit
/// Unit-of-Work transaction (begin -> handler -> commit/rollback).
/// </summary>
public interface ITransactionalRequest
{
}