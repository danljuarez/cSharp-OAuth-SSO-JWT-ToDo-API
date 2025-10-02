using OauthSSOJwtTodoApiBackend.Models.Entities;

namespace OauthSSOJwtTodoApiBackend.Helpers
{
    public interface IJwtHelper
    {
        string GenerateAccessToken(User user);
    }
}