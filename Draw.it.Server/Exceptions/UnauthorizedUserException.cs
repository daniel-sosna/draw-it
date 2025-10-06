using System.Net;

namespace Draw.it.Server.Exceptions;

public class UnauthorizedUserException : AppException
{
    public UnauthorizedUserException(string message) : base(message, HttpStatusCode.Unauthorized) { }
}