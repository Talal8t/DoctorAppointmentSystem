using DAMS.DBAccess;
using DAMS.Models;

using DAMS.Models;

namespace DAMS.Services
{
    public class AuthService
    {
        private readonly AuthRep _authRep;

        public AuthService(AuthRep authRep)
        {
            _authRep = authRep;
        }

        
        public User Login(string email, string password)
        {
            var user = _authRep.GetUserByEmail(email);

            if (user == null)
                return null;

            if (user.Password != password)
                return null;
            Console.WriteLine($"DB Password: '{user.Password}'");
            Console.WriteLine($"Input Password: '{password}'");

            return user;
        }

        
        public void Register(User user)
        {
            var existing = _authRep.GetUserByEmail(user.Email);

            if (existing != null)
                throw new Exception("Email already exists");

            _authRep.Register(user);
        }

        
        public bool IsInRole(User user, string role)
        {
            return user.Role == role;
        }
    }
}
