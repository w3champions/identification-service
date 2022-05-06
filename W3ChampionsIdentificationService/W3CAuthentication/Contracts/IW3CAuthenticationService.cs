namespace W3ChampionsIdentificationService.W3CAuthentication.Contracts
{
    public interface IW3CAuthenticationService
    {
        public W3CUserAuthentication GetUserByToken(string jwt);
    }
}