using System.Net;

namespace Draw.it.Server.Exceptions;

public class EntityNotFoundException : AppException
{
    public EntityNotFoundException(string message) : base(message, HttpStatusCode.NotFound) { }
}