namespace W3ChampionsIdentificationService.WebApi.ExceptionFilters;

public class ErrorResult
{
    public string Error { get; }

    public ErrorResult(string error)
    {
        Error = error;
    }
}
