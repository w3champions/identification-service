namespace W3ChampionsIdentificationService.WebApi.ExceptionFilters;

public class ErrorResult(string error)
{
    public string Error { get; } = error;
}
