using System.Net;

namespace Draw.it.Server.Exceptions;

public class AppException : Exception
{
    public HttpStatusCode Status { get; }

    protected AppException(string message, HttpStatusCode status) : base(message)
    {
        Status = status;
    }
}