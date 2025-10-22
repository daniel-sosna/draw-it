using System.Net;

namespace Draw.it.Server.Exceptions;

public class AppException : Exception
{
    public HttpStatusCode Status { get; }

    public AppException(string message, HttpStatusCode status = HttpStatusCode.InternalServerError) : base(message)
    {
        Status = status;
    }
}