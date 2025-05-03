using OfflineXPlanner.Facade;
using OfflineXPlanner.Facade.Domain;
using OfflineXPlanner.Security;
using System.Threading.Tasks;

namespace OfflineXPlanner.Business
{
    public static class LoginBusiness
    {
        public static bool Login(string username, string password)
        {
            LoginResponse login = LoginFacade.LoginAsync(new LoginRequest(username, password));
            if (login != null)
            {
                AudaxwareRestApiInfo.accessToken = login.access_token;
                return true;
            }

            return false;
        }
    }
}
