namespace GestionClinica.Common;

public record ApiResponse<T>(
    bool Success,
    string Message,
    T? Data = default
);

public static class ApiResponses
{
    public static ApiResponse<T> Ok<T>(T data, string message = "OK") => new(true, message, data);
    public static ApiResponse<T> Fail<T>(string message) => new(false, message);
}
