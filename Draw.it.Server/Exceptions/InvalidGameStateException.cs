using System.Net;

namespace Draw.it.Server.Exceptions;

public class InvalidGameStateException : AppException
{
    public InvalidGameStateException(string message) : base(message, HttpStatusCode.BadRequest) { }
}
