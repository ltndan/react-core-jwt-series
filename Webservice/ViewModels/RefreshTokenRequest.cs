namespace Webservice.ViewModels
{
    public class RefreshTokenRequest
    {
        public string OldJwtToken { get; set; }

        public string RefreshToken { get; set; }
    }
}
