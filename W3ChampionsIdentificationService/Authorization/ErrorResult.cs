namespace W3ChampionsIdentificationService.Authorization
{
    public class ErrorResult
    {
        public string Error { get; }

        public ErrorResult(string error)
        {
            Error = error;
        }
    }
}