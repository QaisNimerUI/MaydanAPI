namespace Maydan.Application.DTOs.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string MessageAr { get; set; } = string.Empty;
    public string MessageEn { get; set; } = string.Empty;
    public T? Data { get; set; }

    public ApiResponse()
    {
    }

    public ApiResponse(bool success, string messageAr, string messageEn, T? data)
    {
        Success = success;
        MessageAr = messageAr;
        MessageEn = messageEn;
        Data = data;
    }
}
