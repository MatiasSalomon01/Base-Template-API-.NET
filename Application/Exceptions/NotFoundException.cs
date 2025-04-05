namespace Application.Exceptions;

public class NotFoundException(string? message = null) : Exception(message ?? "Recurso no entrado.")
{
}
