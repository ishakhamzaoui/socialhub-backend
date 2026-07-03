namespace SocialHub.Application.Common.Results;

/// <summary>
/// Builds a failure Result of whatever concrete TResponse type a pipeline
/// behavior is working with (Result or Result&lt;T&gt;), via reflection over
/// the small, stable Result API surface.
/// </summary>
internal static class ResultFactory
{
    public static TResponse CreateFailure<TResponse>(Error error)
        where TResponse : Result
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        var responseType = typeof(TResponse);
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = responseType.GetGenericArguments()[0];
            var genericFailureMethod = typeof(Result)
                .GetMethods()
                .First(m => m.Name == nameof(Result.Failure) && m.IsGenericMethodDefinition);

            var closedMethod = genericFailureMethod.MakeGenericMethod(valueType);
            return (TResponse)closedMethod.Invoke(null, new object[] { error })!;
        }

        throw new InvalidOperationException($"Unsupported response type '{responseType.Name}' for failure creation.");
    }
}