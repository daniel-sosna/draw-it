using System.Net;

namespace Draw.it.Server.Exceptions;

public class UnauthorizedUserException : AppException
{
    public UnauthorizedUserException(string message = "User ID claim missing.") : base(message, HttpStatusCode.Unauthorized) { }
}