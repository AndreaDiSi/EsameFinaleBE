namespace AutoConfig.Core.Exceptions;

public class AppException(string message, int statusCode = 400) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public class NotFoundException(string resource) : AppException($"{resource} not found.", 404);
public class UnauthorizedException(string message = "Unauthorized.") : AppException(message, 401);
public class ForbiddenException(string message = "Forbidden.") : AppException(message, 403);
public class ConflictException(string message) : AppException(message, 409);
public class ValidationException(string message) : AppException(message, 422);
