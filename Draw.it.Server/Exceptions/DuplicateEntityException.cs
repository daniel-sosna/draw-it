using System.Net;

namespace Draw.it.Server.Exceptions;

public class DuplicateEntityException : AppException
{
    public DuplicateEntityException(string message) : base(message, HttpStatusCode.Conflict) { }
}