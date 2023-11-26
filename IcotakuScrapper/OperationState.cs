namespace IcotakuScrapper;

public readonly struct OperationState
{
    public OperationState() { }
    public OperationState(bool isSuccess, string? message = null)
    {
        IsSuccess = isSuccess;
        Message = message;
    }

    public OperationState(bool isSuccess, string? title, string? message)
    {
        IsSuccess = isSuccess;
        Title = title;
        Message = message;
    }

    public bool IsSuccess { get; init; }
    public string? Title { get; init; }
    public string? Message { get; init; }
}

public readonly struct OperationState<T>
{
    public OperationState()
    {

    }

    public OperationState(bool isSuccess, string? message)
    {
        IsSuccess = isSuccess;
        Message = message;
    }

    public OperationState(bool isSuccess, string? message, T? result)
    {
        IsSuccess = isSuccess;
        Message = message;
        Data = result;
    }

    public OperationState(bool isSuccess, string? title, string? message)
    {
        IsSuccess = isSuccess;
        Title = title;
        Message = message;
    }

    public OperationState(bool isSuccess, string? title, string? message, T? result)
    {
        IsSuccess = isSuccess;
        Title = title;
        Message = message;
        Data = result;
    }

    public bool IsSuccess { get; init; }
    public string? Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public string? Title { get; init; }

    public OperationState ToBaseState()
    {
        return new (IsSuccess, Title, Message);
    }
}