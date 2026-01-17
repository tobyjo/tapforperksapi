namespace TapForPerksAPI.Common;

public class ErrorDetail
{
    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public Dictionary<string, object>? Details { get; set; }

    public ErrorDetail() { }

    public ErrorDetail(string message, string errorCode, Dictionary<string, object>? details = null)
    {
        Message = message;
        ErrorCode = errorCode;
        Details = details;
    }

    public override string ToString()
    {
        return Message;
    }
}
